const urlPattern = /^(?<protocol>https?):\/\/(?<domain>[^\/]+)\/(?<route>[^?#]*)(?<query>\?[^#]*)?(?<hash>#.*)?/i;
const version = '0.0.1';

function fileSearch(event, query) {
  if (query === '?') {
    event.respondWith(fetch(event.request));
    return;
  }

  event.respondWith(fetch(event.request));
}

function flushCache(cache) {

  //clear view model
  cache.delete('/ViewModel.json');
  cache.delete('/ViewModel');

  //clear cached files
  return cache.keys().then(requests => {
    
    requests.forEach(request => {
      let match = urlPattern.exec(request.url);
      if (match?.groups?.route && /^api\/files/i.test(match?.groups?.route)) {
        cache.delete(key);
      }
    });

    return cache;
  });
}

function login(event) {
  event.respondWith(fetch(event.request)
    .then(loginResponse => {

      if (loginResponse.type !== 'opaqueredirect') {
        return loginResponse;
      }
      
      return fetch('/ViewModel')
        .then(viewModelResponse => {

          if (!viewModelResponse.ok) {
            return loginResponse;
          }

          return caches.open(version).then(flushCache).then(cache => {
            
            cache.put('/ViewModel', viewModelResponse.clone());

            return viewModelResponse.text()
              .then(script => {
  
                var json = script.substring(script.indexOf('{'));
                json = json.substring(0, json.lastIndexOf('}') + 1);

                let response = new Response(json, {
                  headers: viewModelResponse.headers,
                  status: viewModelResponse.status,
                  statusText: viewModelResponse.statusText
                });

                cache.put('/ViewModel.json', response);
  
                return loginResponse;
              });
          });
        });
  }));
}

function logout(event) {
  event.respondWith(fetch(event.request)
    .then(logoutResponse => logoutResponse.type !== 'opaqueredirect' ? logoutResponse : caches.open(version)
      .then(cache => flushCache(cache))
      .then(() => logoutResponse)));
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
      login(event, `${protocol}://${domain}/`);
    } else if (route === 'logout') {
      logout(event, `${protocol}://${domain}/`);
    } else {
      event.respondWith(fetch(event.request));
    }
  } else if (method === 'GET') {
    if (route.startsWith('media')) {
      event.respondWith(caches.match(route === 'media/favicon.ico' ? `${protocol}://${domain}/favicon.ico` : `${protocol}://${domain}/Media`));
    } else if (route === 'api/files/search') {
      fileSearch(event, query);
    } else {
      event.respondWith(caches.match(event.request).then(response => response || fetch(event.request)));
    }
  } else {
    event.respondWith(fetch(event.request));
  }
});
