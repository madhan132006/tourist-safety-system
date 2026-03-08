let map;
let dangerZones = [];

function initMap(initialPos, zones) {
    map = new google.maps.Map(document.getElementById("map"), {
        center: { lat: initialPos.latitude, lng: initialPos.longitude },
        zoom: 12,
    });

    zones.forEach(z => {
        new google.maps.Circle({
            strokeColor: '#FF0000',
            strokeOpacity: 0.8,
            strokeWeight: 2,
            fillColor: '#FF0000',
            fillOpacity: 0.35,
            map,
            center: { lat: z.lat, lng: z.lng },
            radius: z.radius,
        });
    });
}

function addDangerZone(lat, lng, radius) {
    dangerZones.push({ lat, lng, radius });
    new google.maps.Circle({
        strokeColor: '#FF0000',
        strokeOpacity: 0.8,
        strokeWeight: 2,
        fillColor: '#FF0000',
        fillOpacity: 0.35,
        map,
        center: { lat, lng },
        radius,
    });
}

function watchPosition(touristId) {
    if (navigator.geolocation) {
        navigator.geolocation.watchPosition(pos => {
            const lat = pos.coords.latitude;
            const lng = pos.coords.longitude;
            fetch('/Home/UpdateLocation', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ touristId, lat, lng })
            });
        });
    }
}