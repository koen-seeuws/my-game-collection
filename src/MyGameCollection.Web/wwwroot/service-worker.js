// Minimal service worker: makes the app installable and caches static assets.
// Blazor Server needs a live connection, so pages and data are always fetched from the server.
const CACHE = 'game-vault-v1';
const STATIC = /\/(_content|_framework|lib|js|icons)\/|\.(css|js|woff2?|png|svg|webmanifest)$/;

self.addEventListener('install', () => self.skipWaiting());

self.addEventListener('activate', event => {
    event.waitUntil((async () => {
        const keys = await caches.keys();
        await Promise.all(keys.filter(k => k !== CACHE).map(k => caches.delete(k)));
        await self.clients.claim();
    })());
});

self.addEventListener('fetch', event => {
    const req = event.request;
    const url = new URL(req.url);
    if (req.method !== 'GET' || url.origin !== self.location.origin) return;
    if (url.pathname.startsWith('/_blazor') || url.pathname.startsWith('/photos/') || !STATIC.test(url.pathname)) return;

    // Stale-while-revalidate for static assets.
    event.respondWith((async () => {
        const cache = await caches.open(CACHE);
        const cached = await cache.match(req);
        const network = fetch(req).then(res => {
            if (res.ok) cache.put(req, res.clone());
            return res;
        }).catch(() => cached);
        return cached || network;
    })());
});
