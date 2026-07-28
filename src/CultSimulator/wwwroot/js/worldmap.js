// World map management using Leaflet.js
// Handles map initialization, coven markers, and mobile pinch-zoom

let worldMap = null;
let covenMarkers = [];
let expandedMarkerId = null;

window.initWorldMap = function(containerId, locations) {
    if (worldMap) {
        worldMap.remove();
        worldMap = null;
        covenMarkers = [];
        expandedMarkerId = null;
    }

    const container = document.getElementById(containerId);
    if (!container) return;

    // Create map with dark theme, mobile-friendly zoom
    worldMap = L.map(container, {
        center: [30, 0],
        zoom: 2,
        minZoom: 2,
        maxZoom: 18,
        zoomControl: true,
        scrollWheelZoom: true,
        // Enable touch zoom (pinch) for mobile
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
            html: '<div class="coven-marker" data-id="' + loc.id + '">' +
                  '<span>' + loc.flag + '</span>' +
                  '<span class="coven-marker-label">' + loc.name + '</span>' +
                  '</div>',
            iconSize: [36, 36],
            iconAnchor: [18, 18]
        });

        const marker = L.marker([loc.lat, loc.lng], { icon: icon }).addTo(worldMap);

        marker.on('click', function() {
            expandMarker(loc.id);
            showCovenPopup(loc);
        });

        covenMarkers.push({ id: loc.id, marker: marker, loc: loc });
    });

    // Fix tile rendering after container becomes visible
    setTimeout(function() {
        if (worldMap) worldMap.invalidateSize();
    }, 200);
};

function expandMarker(id) {
    // Collapse previously expanded marker
    covenMarkers.forEach(function(m) {
        const el = m.marker.getElement();
        if (el) {
            const inner = el.querySelector('.coven-marker');
            if (inner) inner.classList.remove('expanded');
        }
    });

    // Expand the clicked marker
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

function showCovenPopup(loc) {
    const popup = document.getElementById('coven-popup');
    if (!popup) return;

    popup.style.display = 'block';
    popup.innerHTML = '';

    const closeBtn = document.createElement('button');
    closeBtn.className = 'coven-popup-close';
    closeBtn.textContent = '×';
    closeBtn.onclick = function() {
        popup.style.display = 'none';
        collapseAllMarkers();
    };
    popup.appendChild(closeBtn);

    const flag = document.createElement('div');
    flag.className = 'coven-popup-flag';
    flag.textContent = loc.flag;
    popup.appendChild(flag);

    const name = document.createElement('h3');
    name.className = 'coven-popup-name';
    name.textContent = loc.name;
    popup.appendChild(name);

    const location = document.createElement('p');
    location.className = 'coven-popup-location';
    location.textContent = loc.location + ', ' + loc.country;
    popup.appendChild(location);

    const era = document.createElement('p');
    era.className = 'coven-popup-era';
    era.textContent = loc.era;
    popup.appendChild(era);

    const summary = document.createElement('p');
    summary.className = 'coven-popup-summary';
    summary.textContent = loc.summary;
    popup.appendChild(summary);

    const loreToggle = document.createElement('button');
    loreToggle.className = 'coven-popup-lore-toggle';
    loreToggle.textContent = 'Reveal Lore';
    loreToggle.onclick = function() {
        if (loreToggle.textContent === 'Reveal Lore') {
            const lore = document.createElement('p');
            lore.className = 'coven-popup-lore';
            lore.textContent = loc.lore;
            popup.insertBefore(lore, loreToggle);
            loreToggle.textContent = 'Hide Lore';
        } else {
            const loreEl = popup.querySelector('.coven-popup-lore');
            if (loreEl) loreEl.remove();
            loreToggle.textContent = 'Reveal Lore';
        }
    };
    popup.appendChild(loreToggle);
}

function collapseAllMarkers() {
    covenMarkers.forEach(function(m) {
        const el = m.marker.getElement();
        if (el) {
            const inner = el.querySelector('.coven-marker');
            if (inner) inner.classList.remove('expanded');
        }
    });
    expandedMarkerId = null;
}

window.destroyWorldMap = function() {
    if (worldMap) {
        worldMap.remove();
        worldMap = null;
        covenMarkers = [];
        expandedMarkerId = null;
    }
};
