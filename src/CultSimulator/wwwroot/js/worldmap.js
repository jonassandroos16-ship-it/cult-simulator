// ─── World Map (Coven Takeover) ───

var worldMap = null;
var worldMapLayer = null;
var worldMapMarkers = [];
var expandedMarkerId = null;
var shadowWarLayer = null;
var shadowWarMarkers = [];
var shadowWarRivalMarkers = [];

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

// ─── Shadow War Map (Institutions + Rivals) ───

window.initShadowWarMap = function(containerId, institutions, rivals, dotNetRef, options) {
    if (worldMap) worldMap.remove();

    var opts = options || {};
    var zoom = opts.zoom || 2;
    var minZoom = opts.minZoom || 2;
    var center = opts.center || [30, 10];

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

    shadowWarLayer = L.layerGroup().addTo(worldMap);
    shadowWarMarkers = [];
    shadowWarRivalMarkers = [];

    if (institutions) {
        institutions.forEach(function(inst) {
            if (!inst.lat || !inst.lng) return;
            var html = buildShadowWarMarkerHtml(inst);
            var icon = L.divIcon({
                html: html,
                className: 'shadow-war-marker-wrapper',
                iconSize: [40, 40],
                iconAnchor: [20, 20]
            });
            var marker = L.marker([inst.lat, inst.lng], { icon: icon }).addTo(shadowWarLayer);

            marker.on('click', function() {
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('SelectInstitution', inst.id);
                }
                expandShadowWarMarker(inst.id);
            });

            shadowWarMarkers.push({ id: inst.id, marker: marker, inst: inst });
        });
    }

    if (rivals) {
        rivals.forEach(function(rival) {
            if (!rival.lat || !rival.lng) return;
            var html = buildRivalMarkerHtml(rival);
            var icon = L.divIcon({
                html: html,
                className: 'rival-marker-wrapper',
                iconSize: [40, 40],
                iconAnchor: [20, 20]
            });

            // Offset rival markers so they don't overlap institution markers
            var offsetLat = rival.lat + 3;
            var offsetLng = rival.lng + 5;

            var marker = L.marker([offsetLat, offsetLng], { icon: icon }).addTo(worldMap);

            marker.on('click', function() {
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('SelectRival', rival.id);
                }
            });

            shadowWarRivalMarkers.push({ id: rival.id, marker: marker, rival: rival });
        });
    }

    setTimeout(function() {
        if (worldMap) worldMap.invalidateSize();
    }, 200);
};

function buildShadowWarMarkerHtml(inst) {
    var cls = 'shadow-war-marker';
    if (inst.status === 'controlled') cls += ' controlled';
    if (inst.status === 'infiltrating') cls += ' infiltrating';
    if (inst.status === 'recon') cls += ' recon';
    if (inst.status === 'alerted') cls += ' alerted';
    if (inst.status === 'investigated') cls += ' investigated';
    if (inst.status === 'locked') cls += ' locked';
    if (inst.rivalControlled) cls += ' rival-controlled';
    var badge = inst.status === 'controlled' ? '✅' : inst.rivalControlled ? '🔴' : '';
    return '<div class="' + cls + '" data-id="' + inst.id + '">' +
           '<span class="shadow-war-marker-icon">' + inst.icon + '</span>' +
           (badge ? '<span class="shadow-war-marker-badge">' + badge + '</span>' : '') +
           '<span class="shadow-war-marker-label">' + inst.name + '</span>' +
           '</div>';
}

function buildRivalMarkerHtml(rival) {
    var cls = 'rival-marker';
    if (rival.status === 'atwar') cls += ' at-war';
    if (rival.status === 'expanding') cls += ' expanding';
    return '<div class="' + cls + '" data-id="' + rival.id + '">' +
           '<span class="rival-marker-icon">' + rival.icon + '</span>' +
           '<span class="rival-marker-label">' + rival.name + '</span>' +
           '<span class="rival-marker-power">' + Math.floor(rival.power) + '</span>' +
           '</div>';
}

function expandShadowWarMarker(id) {
    shadowWarMarkers.forEach(function(m) {
        var el = m.marker.getElement();
        if (el) {
            var inner = el.querySelector('.shadow-war-marker');
            if (inner) inner.classList.remove('expanded');
        }
    });

    var found = shadowWarMarkers.find(function(m) { return m.id === id; });
    if (found) {
        var el = found.marker.getElement();
        if (el) {
            var inner = el.querySelector('.shadow-war-marker');
            if (inner) inner.classList.add('expanded');
        }
    }
    expandedMarkerId = id;
}

window.destroyShadowWarMap = function() {
    if (worldMap) {
        worldMap.remove();
        worldMap = null;
        shadowWarLayer = null;
        shadowWarMarkers = [];
        shadowWarRivalMarkers = [];
        expandedMarkerId = null;
    }
};
