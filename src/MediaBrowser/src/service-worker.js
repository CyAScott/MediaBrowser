self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open('v1').then((cache) => {
      return cache.addAll([
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
        '/Media/favicon.ico',
        '/polyfills.js',
        '/styles.css',
        '/runtime.js',
        '/vendor.js'
      ]);
    })
  );
});

const mediaRoutePattern = /^(?<base>https?:\/\/[^\/]+)(?<route>\/Media.*)/gmi;

self.addEventListener('fetch', event => {
  if (event.request?.method !== 'GET' || !event.request?.url) {
    event.respondWith(fetch(event.request));
    return;
  }
  
  let match = mediaRoutePattern.exec(event.request.url);
  if (match?.groups?.route && match.groups.route.toLowerCase() !== '/media/favicon.ico') {
    event.respondWith(caches.match(`${match.groups.base}/Media`));
    return;
  }

  event.respondWith(caches.match(event.request).then(response => response || fetch(event.request)));
});
