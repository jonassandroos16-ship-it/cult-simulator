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

// ---- Ley Line map mode ----
let leyLineLayer = null;
let leyLineMarkers = [];
let leyLineConnections = [];
let leyLineSeals = [];

window.initLeyLineMap = function(containerId, nodes, connections, seals, dotNetHelper, options) {
    if (worldMap) {
        worldMap.remove();
        worldMap = null;
        covenMarkers = [];
        expandedMarkerId = null;
    }
    leyLineLayer = null;
    leyLineMarkers = [];
    leyLineConnections = [];
    leyLineSeals = [];

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

    leyLineLayer = L.layerGroup().addTo(worldMap);

    // Draw connection polylines first (behind markers)
    if (connections) {
        connections.forEach(function(conn) {
            var nodeA = nodes.find(function(n) { return n.id === conn[0]; });
            var nodeB = nodes.find(function(n) { return n.id === conn[1]; });
            if (nodeA && nodeB) {
                var line = L.polyline([[nodeA.lat, nodeA.lng], [nodeB.lat, nodeB.lng]], {
                    color: '#a78bfa',
                    weight: 2,
                    opacity: 0.6,
                    className: 'ley-line-connection'
                }).addTo(leyLineLayer);
                leyLineConnections.push(line);
            }
        });
    }

    // Draw seal triangles (filled polygons)
    if (seals) {
        seals.forEach(function(seal) {
            var points = seal.nodeIds.map(function(id) {
                var n = nodes.find(function(nn) { return nn.id === id; });
                return n ? [n.lat, n.lng] : null;
            }).filter(Boolean);
            if (points.length === 3) {
                var polygon = L.polygon(points, {
                    color: '#fbbf24',
                    weight: 1,
                    opacity: 0.5,
                    fillColor: '#fbbf24',
                    fillOpacity: 0.08,
                    className: 'ley-line-seal',
                    dashArray: '4,4'
                }).addTo(leyLineLayer);
                leyLineSeals.push(polygon);

                // Add multiplier label at centroid
                var lat = (points[0][0] + points[1][0] + points[2][0]) / 3;
                var lng = (points[0][1] + points[1][1] + points[2][1]) / 3;
                L.marker([lat, lng], {
                    icon: L.divIcon({
                        className: 'ley-line-seal-label',
                        html: '<div class="seal-label">×' + seal.multiplier.toFixed(2) + '</div>',
                        iconSize: [60, 24],
                        iconAnchor: [30, 12]
                    }),
                    interactive: false
                }).addTo(leyLineLayer);
            }
        });
    }

    // Draw node markers
    if (nodes) {
        nodes.forEach(function(node) {
            var html = buildLeyLineMarkerHtml(node);
            var icon = L.divIcon({
                className: 'ley-line-marker-wrapper',
                html: html,
                iconSize: [40, 40],
                iconAnchor: [20, 20]
            });

            var marker = L.marker([node.lat, node.lng], { icon: icon }).addTo(worldMap);

            marker.on('click', function() {
                expandLeyLineMarker(node.id);
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('SelectLeyLineNode', node.id);
                }
            });

            leyLineMarkers.push({ id: node.id, marker: marker, node: node });
        });
    }

    setTimeout(function() {
        if (worldMap) worldMap.invalidateSize();
    }, 200);
};

function buildLeyLineMarkerHtml(node) {
    var cls = 'ley-line-marker';
    if (node.conquered) cls += ' conquered';
    if (node.canConquer) cls += ' available';
    if (node.veiled) cls += ' veiled';
    var badge = node.canConquer && !node.conquered ? '<span class="ley-line-marker-target">⚡</span>' : '';
    return '<div class="' + cls + '" data-id="' + node.id + '">' +
           '<span class="ley-line-marker-icon">' + node.icon + '</span>' +
           badge +
           '<span class="ley-line-marker-label">' + node.name + '</span>' +
           '</div>';
}

function expandLeyLineMarker(id) {
    leyLineMarkers.forEach(function(m) {
        var el = m.marker.getElement();
        if (el) {
            var inner = el.querySelector('.ley-line-marker');
            if (inner) inner.classList.remove('expanded');
        }
    });

    var found = leyLineMarkers.find(function(m) { return m.id === id; });
    if (found) {
        var el = found.marker.getElement();
        if (el) {
            var inner = el.querySelector('.ley-line-marker');
            if (inner) inner.classList.add('expanded');
        }
    }
    expandedMarkerId = id;
}

window.destroyLeyLineMap = function() {
    if (worldMap) {
        worldMap.remove();
        worldMap = null;
        leyLineLayer = null;
        leyLineMarkers = [];
        leyLineConnections = [];
        leyLineSeals = [];
        expandedMarkerId = null;
    }
};
