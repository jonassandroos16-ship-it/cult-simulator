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

    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; OpenStreetMap &copy; CARTO',
        subdomains: 'abcd',
        maxZoom: 19
    }).addTo(worldMap);

    locations.forEach(function(loc) {
        var cls = 'coven-marker';
        if (loc.takenOver) cls += ' taken-over';
        if (loc.isNextTarget) cls += ' next-target';
        if (loc.isActive) cls += ' active-coven';
        if (loc.locked) cls += ' locked';
        var badge = loc.isNextTarget ? '<span class="coven-marker-target">🎯</span>' : '';
        if (loc.locked) badge = '<span class="coven-marker-target">🔒</span>';
        var html = '<div class="' + cls + '" data-id="' + loc.id + '">' +
                   '<span class="coven-marker-flag">' + loc.flag + '</span>' +
                   badge +
                   '<span class="coven-marker-label">' + loc.name + '</span>' +
                   '</div>';

        const icon = L.divIcon({
            className: 'coven-marker-wrapper',
            html: html,
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

    setTimeout(function() {
        if (worldMap) worldMap.invalidateSize();
    }, 200);
};

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

// ---- Shadow War map mode ----
let shadowWarLayer = null;
let shadowWarMarkers = [];
let shadowWarRivalMarkers = [];

window.initShadowWarMap = function(containerId, institutions, rivals, dotNetHelper, options) {
    if (worldMap) {
        worldMap.remove();
        worldMap = null;
        covenMarkers = [];
        expandedMarkerId = null;
    }
    shadowWarLayer = null;
    shadowWarMarkers = [];
    shadowWarRivalMarkers = [];

    dotNetRef = dotNetHelper || null;

    const container = document.getElementById(containerId);
    if (!container) return;

    var opts = options || {};

    worldMap = L.map(container, {
        center: opts.center || [30, 10],
        zoom: opts.zoom || 2,
        minZoom: opts.minZoom || 2,
        maxZoom: 18,
        zoomControl: true,
        scrollWheelZoom: true,
        touchZoom: true,
        tap: true,
        worldCopyJump: true,
        attributionControl: true
    });

    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; OpenStreetMap &copy; CARTO',
        subdomains: 'abcd',
        maxZoom: 19
    }).addTo(worldMap);

    shadowWarLayer = L.layerGroup().addTo(worldMap);

    if (institutions) {
        institutions.forEach(function(inst) {
            var html = buildShadowWarMarkerHtml(inst);
            var icon = L.divIcon({
                className: 'shadow-war-marker-wrapper',
                html: html,
                iconSize: [44, 44],
                iconAnchor: [22, 22]
            });

            var marker = L.marker([inst.lat, inst.lng], { icon: icon }).addTo(worldMap);

            marker.on('click', function() {
                expandShadowWarMarker(inst.id);
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('SelectInstitution', inst.id);
                }
            });

            shadowWarMarkers.push({ id: inst.id, marker: marker, inst: inst });
        });
    }

    if (rivals) {
        rivals.forEach(function(rival) {
            if (!rival.lat || !rival.lng) return;
            var html = buildRivalMarkerHtml(rival);
            var icon = L.divIcon({
                className: 'rival-marker-wrapper',
                html: html,
                iconSize: [40, 40],
                iconAnchor: [20, 20]
            });

            // Offset rival markers so they don't overlap institution markers
            var offsetLat = rival.lat + 3;
            var offsetLng = rival.lng + 5;

            var marker = L.marker([offsetLat, offsetLng], { icon: icon }).addTo(worldMap);
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
