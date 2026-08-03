// ─── World Map (Coven Takeover) ───

var worldMap = null;
var worldMapLayer = null;
var worldMapMarkers = [];
var expandedMarkerId = null;

window.initWorldMap = function(containerId, locations, dotNetRef, options) {
    if (worldMap) worldMap.remove();

    var opts = options || {};
    var zoom = opts.zoom || 3;
    var minZoom = opts.minZoom || 2;
    var center = opts.center || [20, 10];

    worldMap = L.map(containerId, {
        zoomControl: true,
        minZoom: minZoom,
        maxBounds: [[-85, -180], [85, 180]],
        worldCopyJump: true
    }).setView(center, zoom);

    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; OpenStreetMap &copy; CARTO',
        subdomains: 'abcd',
        maxZoom: 19
    }).addTo(worldMap);

    worldMapLayer = L.layerGroup().addTo(worldMap);
    worldMapMarkers = [];

    locations.forEach(function(loc) {
        if (!loc.lat || !loc.lng) return;
        var html = buildMarkerHtml(loc);
        var icon = L.divIcon({
            html: html,
            className: 'coven-marker-wrapper',
            iconSize: [44, 44],
            iconAnchor: [22, 22]
        });
        var marker = L.marker([loc.lat, loc.lng], { icon: icon }).addTo(worldMapLayer);

        marker.on('click', function() {
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('SelectCoven', loc.id);
            }
            expandMarker(loc.id);
        });

        worldMapMarkers.push({ id: loc.id, marker: marker, loc: loc });
    });

    setTimeout(function() {
        if (worldMap) worldMap.invalidateSize();
    }, 200);
};

function buildMarkerHtml(loc) {
    var cls = 'coven-marker';
    if (loc.takenOver) cls += ' taken-over';
    if (loc.isNextTarget) cls += ' next-target';
    if (loc.isActive) cls += ' active-coven';
    if (loc.locked) cls += ' locked';
    var targetBadge = loc.isNextTarget ? '<span class="coven-marker-target">🎯</span>' : '';
    return '<div class="' + cls + '" data-id="' + loc.id + '">' +
           '<span class="coven-marker-flag">' + (loc.flag || '📍') + '</span>' +
           targetBadge +
           '<span class="coven-marker-label">' + loc.name + '</span>' +
           '</div>';
}

function expandMarker(id) {
    worldMapMarkers.forEach(function(m) {
        var el = m.marker.getElement();
        if (el) {
            var inner = el.querySelector('.coven-marker');
            if (inner) inner.classList.remove('expanded');
        }
    });

    var found = worldMapMarkers.find(function(m) { return m.id === id; });
    if (found) {
        var el = found.marker.getElement();
        if (el) {
            var inner = el.querySelector('.coven-marker');
            if (inner) inner.classList.add('expanded');
        }
    }
    expandedMarkerId = id;
}

window.destroyWorldMap = function() {
    if (worldMap) {
        worldMap.remove();
        worldMap = null;
        worldMapLayer = null;
        worldMapMarkers = [];
        expandedMarkerId = null;
    }
};
