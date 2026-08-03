const CACHE_NAME = "cultsim-v2";
const BASE = "/cult-simulator/";
const ASSETS = [
  BASE,
  BASE + "index.html",
  BASE + "icon.svg",
  BASE + "manifest.json",
  BASE + "css/base.css",
  BASE + "css/loading.css",
  BASE + "css/naming.css",
  BASE + "css/layout.css",
  BASE + "css/altar.css",
  BASE + "css/panels.css",
  BASE + "css/events.css",
  BASE + "css/tabs.css",
  BASE + "css/worldmap.css",
  BASE + "css/rankup.css",
  BASE + "css/themes.css",
  BASE + "css/tailwind.css",
  BASE + "js/worldmap.js",
  BASE + "js/autosave.js",
  BASE + "js/supabase-auth.js",
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

  if (url.pathname.startsWith("/_framework") || url.pathname.startsWith("/_blazor")) {
    return;
  }

  if (req.mode === "navigate") {
    e.respondWith(
      fetch(req)
        .then((resp) => {
          const copy = resp.clone();
          caches.open(CACHE_NAME).then((c) => c.put(req, copy)).catch(() => {});
          return resp;
        })
        .catch(() => caches.match(req).then((r) => r || caches.match(BASE)))
    );
    return;
  }

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
