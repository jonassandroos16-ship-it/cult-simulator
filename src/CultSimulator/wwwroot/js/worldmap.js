// World map management using Leaflet.js
// Handles map initialization, coven markers, and mobile pinch-zoom

let worldMap = null;
let covenMarkers = [];
let expandedMarkerId = null;
let dotNetRef = null;

window.initWorldMap = function(containerId, locations, dotNetHelper, options) {
    if (worldMap) {
        worldMap.remove();
        worldMap = null;
        covenMarkers = [];
        expandedMarkerId = null;
    }

    dotNetRef = dotNetHelper || null;

    const container = document.getElementById(containerId);
    if (!container) return;

    var opts = options || {};
    var initialZoom = opts.zoom || 2;

    // Create map with dark theme, mobile-friendly zoom
    worldMap = L.map(container, {
        center: opts.center || [30, 0],
        zoom: initialZoom,
        minZoom: opts.minZoom || 2,
        maxZoom: 18,
        zoomControl: true,
        scrollWheelZoom: true,
        touchZoom: true,
        tap: true,
        worldCopyJump: true,
        attributionControl: true
    });

    // Dark-themed tile layer
    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; OpenStreetMap &copy; CARTO',
        subdomains: 'abcd',
        maxZoom: 19
    }).addTo(worldMap);

    // Add a coven marker for each location
    locations.forEach(function(loc) {
        const icon = L.divIcon({
            className: 'coven-marker-wrapper',
            html: buildMarkerHtml(loc),
            iconSize: [36, 36],
            iconAnchor: [18, 18]
        });

        const marker = L.marker([loc.lat, loc.lng], { icon: icon }).addTo(worldMap);

        marker.on('click', function() {
            expandMarker(loc.id);
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('SelectCoven', loc.id);
            }
        });

        covenMarkers.push({ id: loc.id, marker: marker, loc: loc });
    });

    // Fix tile rendering after container becomes visible
    setTimeout(function() {
        if (worldMap) worldMap.invalidateSize();
    }, 200);
};

function buildMarkerHtml(loc) {
    var cls = 'coven-marker';
    if (loc.takenOver) cls += ' taken-over';
    if (loc.isNextTarget) cls += ' next-target';
    if (loc.isActive) cls += ' active-coven';
    var badge = loc.isNextTarget ? '<span class="coven-marker-target">🎯</span>' : '';
    return '<div class="' + cls + '" data-id="' + loc.id + '">' +
           '<span class="coven-marker-flag">' + loc.flag + '</span>' +
           badge +
           '<span class="coven-marker-label">' + loc.name + '</span>' +
           '</div>';
}

function expandMarker(id) {
    covenMarkers.forEach(function(m) {
        const el = m.marker.getElement();
        if (el) {
            const inner = el.querySelector('.coven-marker');
            if (inner) inner.classList.remove('expanded');
        }
    });

    const found = covenMarkers.find(function(m) { return m.id === id; });
    if (found) {
        const el = found.marker.getElement();
        if (el) {
            const inner = el.querySelector('.coven-marker');
            if (inner) inner.classList.add('expanded');
        }
    }
    expandedMarkerId = id;
}

window.destroyWorldMap = function() {
    if (worldMap) {
        worldMap.remove();
        worldMap = null;
        covenMarkers = [];
        expandedMarkerId = null;
    }
};
