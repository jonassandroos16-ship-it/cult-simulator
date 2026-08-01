const CACHE_NAME = "cultsim-v1";
const ASSETS = [
  "/",
  "/index.html",
  "/manifest.json",
  "/favicon.png",
  "/icon-192.png",
  "/icon-512.png",
  "/css/base.css",
  "/css/loading.css",
  "/css/naming.css",
  "/css/layout.css",
  "/css/altar.css",
  "/css/panels.css",
  "/css/events.css",
  "/css/tabs.css",
  "/css/worldmap.css",
  "/css/rankup.css",
  "/css/shadowwar.css",
  "/css/themes.css",
  "/css/tailwind.css",
  "/js/worldmap.js",
  "/js/autosave.js",
  "https://unpkg.com/leaflet@1.9.4/dist/leaflet.css",
  "https://unpkg.com/leaflet@1.9.4/dist/leaflet.js",
  "https://fonts.googleapis.com/css2?family=Cinzel:wght@400;500;600;700&family=Inter:wght@300;400;500;600&display=swap"
];

self.addEventListener("install", (e) => {
  e.waitUntil(
    caches.open(CACHE_NAME).then((cache) =>
      cache.addAll(ASSETS).catch(() => {})
    )
  );
  self.skipWaiting();
});

self.addEventListener("activate", (e) => {
  e.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((k) => k !== CACHE_NAME).map((k) => caches.delete(k)))
    )
  );
  self.clients.claim();
});

self.addEventListener("fetch", (e) => {
  const req = e.request;
  if (req.method !== "GET") return;

  const url = new URL(req.url);

  // Never cache Blazor framework files or API calls
  if (url.pathname.startsWith("/_framework") || url.pathname.startsWith("/_blazor")) {
    return;
  }

  // Network-first for navigation requests (so users get latest HTML)
  if (req.mode === "navigate") {
    e.respondWith(
      fetch(req)
        .then((resp) => {
          const copy = resp.clone();
          caches.open(CACHE_NAME).then((c) => c.put(req, copy)).catch(() => {});
          return resp;
        })
        .catch(() => caches.match(req).then((r) => r || caches.match("/")))
    );
    return;
  }

  // Cache-first for everything else
  e.respondWith(
    caches.match(req).then((cached) => {
      if (cached) return cached;
      return fetch(req).then((resp) => {
        if (resp.ok && (url.origin === self.location.origin || url.protocol === "https:")) {
          const copy = resp.clone();
          caches.open(CACHE_NAME).then((c) => c.put(req, copy)).catch(() => {});
        }
        return resp;
      }).catch(() => cached);
    })
  );
});