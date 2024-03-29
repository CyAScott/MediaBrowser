self.importScripts('zangodb.min.js');

const urlPattern = /^(?<protocol>https?):\/\/(?<domain>[^\/]+)\/(?<route>[^?#]*)(?<query>\?[^#]*)?(?<hash>#.*)?/i;
const localFilePattern = /^local\/cache\/(?<fileId>[a-f\d]{8}(-[a-f\d]{4}){3}-[a-f\d]{12})$/i;
const version = '0.0.1';

let db = new zango.Db('cache', { files: ['id'] });
let files = db.collection('files');

async function cachedFilesSearch(request) {
  let response = {
    ascending: request.ascending?.toLowerCase() === 'true',
    filter: request.filter,
    keywords: request.keywords,
    skip: request.skip && /\d+/.test(request.skip) ? request.skip * 1 : 0,
    sort: request.sort,
    take: Math.max(request.take && /\d+/.test(request.take) ? request.take * 1 : 1, 1)
  };
  let query = {};
  let sort = {};

  if (response.keywords && /\S/.test(response.keywords)) {
    let keywords = response.keywords.split(/\s+/).filter(word => word.length > 0).map(escapeRegExp);

    if (keywords.length) {
      var regex = new RegExp(keywords.length === 1 ? keywords[0] : '(' + keywords.join('|') + ')');
      query = {'$or': [ {description: {'$regex': regex, '$options': 'i'}}, {name: {'$regex': regex, '$options': 'i'}} ]};
    }
  }

  switch (request.sort?.toLowerCase() || 'name') {
    case 'createdon':
      sort.uploadedOn = response.ascending ? 1 : -1;
      break;
    case 'duration':
      sort.duration = response.ascending ? 1 : -1;
      break;
    default://name
      sort.name = response.ascending ? 1 : -1;
      break;
    case 'type':
      sort.type = response.ascending ? 1 : -1;
      break;
  }

  response.results = await files.find(query).sort(sort).skip(response.skip).limit(response.take).toArray();

  if (response.results.length < response.take) {
    response.count = response.skip + response.results.length;
  } else {
    let group = await files.find(query).group({_id: 1, count: {'$sum': 1}}).toArray();

    response.count = group[0].count;
  }

  return new Response(JSON.stringify(response), {
    status: 200,
    statusText: 'Ok'
  });
}

async function cacheFile(fileId, base) {
  let file = await fetch(`${base}/api/files/${fileId}`);

  if (!file.ok) {
    return new Response('', {
      status: 404,
      statusText: 'Not Found'
    });
  }

  let fileContents = await fetch(`${base}/api/files/${fileId}/contents`);
  if (!fileContents.ok) {
    return new Response('', {
      status: 404,
      statusText: 'Not Found'
    });
  }

  let cache = await caches.open(version);
  let fileInfo = JSON.parse(await file.text());

  fileInfo.cached = true;

  cache.put(`api/files/${fileId}`, new Response(JSON.stringify(fileInfo), {
    headers: file.headers,
    status: file.status,
    statusText: file.statusText
  }));
  cache.put(`api/files/${fileId}/contents`, fileContents);

  if (fileInfo.thumbnails) {
    let thumbnails = fileInfo.thumbnails;

    for (var index = 0; index < thumbnails.length; index++) {

      let md5 = thumbnails[index].md5;

      let thumbnailContents = await fetch(`${base}/api/files/${fileId}/thumbnails/${md5}/contents`);

      if (thumbnailContents.ok) {
        cache.put(`api/files/${fileId}/thumbnails/${md5}/contents`, thumbnailContents);
      }
    }
  }

  await files.insert(fileInfo);

  return new Response(JSON.stringify(fileInfo), {
    status: 200,
    statusText: 'Ok'
  });
}

async function fileSearch(request, query) {
  if (query === '?') {
    return await fetch(request);
  }

  let queryArgs = {};

  query.substring(1).split('&').forEach(param => {

    let index = param.indexOf('=');
    
    if (index === -1) {
      queryArgs[decodeURIComponent(param).toLowerCase()] = '';
      return;
    }

    queryArgs[decodeURIComponent(param.substring(0, index)).toLowerCase()] = decodeURIComponent(param.substring(index + 1));

  });

  return queryArgs.filter?.toLowerCase() === 'cached' ? await cachedFilesSearch(queryArgs) : await fetch(request);
}

async function flushCache(cache) {

  //clear view model
  cache.delete('/ViewModel.json');
  cache.delete('/ViewModel');

  //clear cached files
  let requests = await cache.keys();

  requests.forEach(request => {
    let match = urlPattern.exec(request.url);
    if (match?.groups?.route && /^api\/files/i.test(match?.groups?.route)) {
      cache.delete(request);
    }
  });

  await files.remove({});

  return cache;
}

async function login(request) {
  let loginResponse = await fetch(request);

  if (loginResponse.type !== 'opaqueredirect') {
    return loginResponse;
  }
  
  let viewModelResponse = await fetch('/ViewModel');

  if (!viewModelResponse.ok) {
    return loginResponse;
  }

  let cache = await caches.open(version);

  await flushCache(cache);

  cache.put('/ViewModel', viewModelResponse.clone());

  let script = await viewModelResponse.text();

  var json = script.substring(script.indexOf('{'));
  json = json.substring(0, json.lastIndexOf('}') + 1);

  cache.put('/ViewModel.json', new Response(json, {
    headers: viewModelResponse.headers,
    status: viewModelResponse.status,
    statusText: viewModelResponse.statusText
  }));

  return loginResponse;
}

async function logout(request) {
  let logoutResponse = await fetch(request);

  if (logoutResponse.type !== 'opaqueredirect') {
    return logoutResponse;
  }

  let cache = await caches.open(version);

  await flushCache(cache);

  return logoutResponse;
}

async function uncacheFile(fileId) {
  let fileInfo = await files.findOne({id: fileId});

  if (!fileInfo) {
    return new Response('', {
      status: 404,
      statusText: 'Not Found'
    });
  }

  let cache = await caches.open(version);

  cache.delete(`api/files/${fileId}`);
  cache.delete(`api/files/${fileId}/contents`);
  fileInfo.thumbnails?.forEach(thumbnail => cache.delete(`api/files/${fileId}/thumbnails/${thumbnail.md5}/contents`));

  await files.remove({id: fileId});

  delete fileInfo.cached;

  return new Response(JSON.stringify(fileInfo), {
    status: 200,
    statusText: 'Ok'
  });
}

function escapeRegExp(text) {
  //copied from: https://stackoverflow.com/questions/3115150/how-to-escape-regular-expression-special-characters-using-javascript
  return text.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, '\\$&');
}

self.addEventListener('install', event => {
  event.waitUntil(caches.open(version)
    .then(cache => cache.addAll([
      '/archivo-narrow-all-400-normal.woff',
      '/archivo-narrow-latin-400-normal.woff2',
      '/archivo-narrow-latin-ext-400-normal.woff2',
      '/archivo-narrow-vietnamese-400-normal.woff2',
      '/fa-brands-400.eot',
      '/fa-brands-400.svg',
      '/fa-brands-400.ttf',
      '/fa-brands-400.woff',
      '/fa-brands-400.woff2',
      '/fa-regular-400.eot',
      '/fa-regular-400.svg',
      '/fa-regular-400.ttf',
      '/fa-regular-400.woff',
      '/fa-regular-400.woff2',
      '/fa-solid-900.eot',
      '/fa-solid-900.svg',
      '/fa-solid-900.ttf',
      '/fa-solid-900.woff',
      '/fa-solid-900.woff2',
      '/favicon.ico',
      '/Login',
      '/main.js',
      '/Media',
      '/polyfills.js',
      '/styles.css',
      '/runtime.js',
      '/vendor.js'
    ])));
});

self.addEventListener('fetch', event => {

  if (!event?.request?.url || !event?.request?.method) {
    event.respondWith(caches.match(event.request).then(response => response || fetch(event.request)));
    return;
  }

  let match = urlPattern.exec(event.request.url);
  if (!match?.groups) {
    event.respondWith(caches.match(event.request).then(response => response || fetch(event.request)));
    return;
  }

  let domain = match.groups.domain;
  let method = event.request.method.toUpperCase();
  let protocol = match.groups.protocol;
  let route = match.groups.route?.toLowerCase() || '';
  let query = match.groups.query || '?';

  if (method === 'POST') {
    if (route === 'login') {
      event.respondWith(login(event.request));
    } else if (route === 'logout') {
      event.respondWith(logout(event.request));
    } else {
      event.respondWith(fetch(event.request));
    }
  } else if (route.startsWith('local/cache/')) {
    let fileId = localFilePattern.exec(route)?.groups?.fileId;
    if (fileId && method === 'GET') {
      event.respondWith(cacheFile(fileId, `${protocol}://${domain}`));
    } else if (fileId && method === 'DELETE') {
      event.respondWith(uncacheFile(fileId));
    } else {
      event.respondWith(fetch(event.request));
    }
  } else if (method === 'GET') {
    if (route.startsWith('media')) {
      event.respondWith(caches.match(route === 'media/favicon.ico' ? `${protocol}://${domain}/favicon.ico` : `${protocol}://${domain}/Media`));
    } else if (route === 'api/files/search') {
      event.respondWith(fileSearch(event.request, query));
    } else {
      event.respondWith(caches.match(event.request).then(response => response || fetch(event.request)));
    }
  } else {
    event.respondWith(fetch(event.request));
  }
});
