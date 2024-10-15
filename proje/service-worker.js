self.addEventListener('install', (event) => {
    console.log('Service Worker yüklendi');
});

self.addEventListener('activate', (event) => {
    console.log('Service Worker çalıştırıldı');
});

self.addEventListener('fetch', (event) => {
    event.respondWith(
        fetch(event.request).catch(() => {
            return new Response('Offline');
        })
    );
});
