self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open('v1').then((cache) => {
      return cache.addAll([
        '/',
        './favicon.ico',
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
