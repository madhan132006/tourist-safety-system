// Tourist Safety System - Enhanced Frontend JavaScript (v2.0)
// With improved error handling, validation, and debugging

const API_BASE_URL = 'http://localhost:5000/api';
const GOOGLE_MAPS_API_KEY = 'YOUR_ACTUAL_GOOGLE_MAPS_API_KEY';

// DOM Elements
const sections = document.querySelectorAll('.section');
const touristsList = document.getElementById('tourists-list');
const incidentsList = document.getElementById('incidents-list');
const dangerZonesList = document.getElementById('danger-zones-list');

// Google Maps variables
let map;
let markers = [];
let dangerZoneCircles = [];
let currentLocationMarker;

// Debug Mode
const DEBUG = true;

function log(message, data = null) {
    if (DEBUG) {
        console.log(`[TouristSafety] ${message}`, data || '');
    }
}

// Initialize the application
document.addEventListener('DOMContentLoaded', function() {
    log('Application starting...');
    setTimeout(() => {
        loadDashboard();
        loadTourists();
        loadIncidents();
        loadDangerZones();
        initializeMap();
        log('All data loaded');
    }, 500);
});

// Navigation
function showSection(sectionId) {
    sections.forEach(section => section.classList.remove('active'));
    document.getElementById(sectionId).classList.add('active');
}

// Dashboard with Error Handling
async function loadDashboard() {
    try {
        log('Loading dashboard data...');
        
        const [tourists, incidents, zones] = await Promise.all([
            fetch(`${API_BASE_URL}/tourists`).then(async r => {
                if (!r.ok) throw new Error(`Tourists API Error: ${r.status}`);
                return r.json();
            }),
            fetch(`${API_BASE_URL}/incidents`).then(async r => {
                if (!r.ok) throw new Error(`Incidents API Error: ${r.status}`);
                return r.json();
            }),
            fetch(`${API_BASE_URL}/dangerzones`).then(async r => {
                if (!r.ok) throw new Error(`Danger Zones API Error: ${r.status}`);
                return r.json();
            })
        ]);

        log('Dashboard data received:', { tourists, incidents, zones });

        const touristCount = tourists.length || 0;
        const activeIncidents = incidents ? incidents.filter(i => !i.isHandled).length : 0;
        const activeZones = zones ? zones.filter(z => z.isActive).length : 0;

        document.getElementById('total-tourists').textContent = touristCount;
        document.getElementById('active-incidents').textContent = activeIncidents;
        document.getElementById('danger-zones-count').textContent = activeZones;
        document.getElementById('risk-alerts').textContent = '0';

        log(`Dashboard Updated: ${touristCount} tourists, ${activeIncidents} incidents, ${activeZones} zones`);

    } catch (error) {
        console.error('❌ Error loading dashboard:', error);
        showMessage(`Dashboard Error: ${error.message} - Is backend running?`, 'error');
        
        document.getElementById('total-tourists').textContent = '0';
        document.getElementById('active-incidents').textContent = '0';
        document.getElementById('danger-zones-count').textContent = '0';
    }
}

// Tourists Management
async function loadTourists() {
    try {
        log('Fetching tourists from API...');
        const response = await fetch(`${API_BASE_URL}/tourists`);
        
        if (!response.ok) {
            throw new Error(`API Error: ${response.status} ${response.statusText}`);
        }

        const tourists = await response.json();
        log('Tourists loaded:', tourists);
        
        displayTourists(tourists || []);
        populateTouristSelect(tourists || []);

    } catch (error) {
        console.error('❌ Error loading tourists:', error);
        touristsList.innerHTML = `<p class="error">⚠️ Error loading tourists: ${error.message}<br><small>Is the backend running on http://localhost:5000?</small></p>`;
    }
}

function displayTourists(tourists) {
    if (!tourists || tourists.length === 0) {
        touristsList.innerHTML = '<p style="text-align: center; color: #666;">No tourists registered yet. Add one using the form above!</p>';
        return;
    }

    touristsList.innerHTML = tourists.map(tourist => `
        <div class="list-item">
            <h4>${tourist.name || 'Unknown'}</h4>
            <p><strong>Email:</strong> ${tourist.email || 'N/A'}</p>
            <p><strong>Phone:</strong> ${tourist.phoneNumber || 'N/A'}</p>
            <p><strong>Age:</strong> ${tourist.age || 'N/A'}</p>
            <p><strong>Nationality:</strong> ${tourist.nationality || 'N/A'}</p>
            <p><strong>Location:</strong> ${(tourist.latitude || 0).toFixed(4)}, ${(tourist.longitude || 0).toFixed(4)}</p>
            <div class="actions">
                <button onclick="editTourist(${tourist.id || 0})">Edit</button>
                <button onclick="deleteTourist(${tourist.id || 0})">Delete</button>
            </div>
        </div>
    `).join('');
}

function populateTouristSelect(tourists) {
    const select = document.getElementById('incident-tourist');
    const trackSelect = document.getElementById('track-tourist');
    const options = '<option value="">Select Tourist</option>' +
        tourists.map(t => `<option value="${t.id}">${t.name}</option>`).join('');

    select.innerHTML = options;
    trackSelect.innerHTML = options;
}

function showTouristForm() {
    document.getElementById('tourist-form').style.display = 'block';
}

function hideTouristForm() {
    document.getElementById('tourist-form').style.display = 'none';
    document.getElementById('add-tourist-form').reset();
}

document.getElementById('add-tourist-form').addEventListener('submit', async function(e) {
    e.preventDefault();

    // Validate required fields
    const name = document.getElementById('tourist-name')?.value?.trim();
    const email = document.getElementById('tourist-email')?.value?.trim();
    const phone = document.getElementById('tourist-phone')?.value?.trim();
    const age = document.getElementById('tourist-age')?.value?.trim();
    const nationality = document.getElementById('tourist-nationality')?.value?.trim();

    if (!name) {
        showMessage('Please enter tourist name', 'error');
        return;
    }
    if (!email || !email.includes('@')) {
        showMessage('Please enter a valid email address', 'error');
        return;
    }
    if (!phone) {
        showMessage('Please enter phone number', 'error');
        return;
    }
    if (!age || isNaN(age) || age < 1 || age > 150) {
        showMessage('Please enter a valid age (1-150)', 'error');
        return;
    }
    if (!nationality) {
        showMessage('Please enter nationality', 'error');
        return;
    }

    const tourist = {
        name: name,
        email: email,
        phoneNumber: phone,
        age: parseInt(age),
        nationality: nationality,
        emergencyContact: document.getElementById('tourist-emergency-contact')?.value || '',
        emergencyPhoneNumber: document.getElementById('tourist-emergency-phone')?.value || '',
        firstTimeVisitor: document.getElementById('first-time-visitor')?.checked || false,
        latitude: 0,
        longitude: 0
    };

    try {
        const response = await fetch(`${API_BASE_URL}/tourists`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(tourist)
        });

        if (response.ok) {
            hideTouristForm();
            loadTourists();
            loadDashboard();
            showMessage('Tourist added successfully', 'success');
        } else {
            throw new Error('Failed to add tourist');
        }
    } catch (error) {
        console.error('Error adding tourist:', error);
        showMessage(`Error adding tourist. Is backend running at ${API_BASE_URL}?`, 'error');
    }
});

// Incidents Management
async function loadIncidents() {
    try {
        log('Fetching incidents from API...');
        const response = await fetch(`${API_BASE_URL}/incidents`);
        
        if (!response.ok) {
            throw new Error(`API Error: ${response.status} ${response.statusText}`);
        }

        const incidents = await response.json();
        log('Incidents loaded:', incidents);
        displayIncidents(incidents || []);

    } catch (error) {
        console.error('❌ Error loading incidents:', error);
        incidentsList.innerHTML = `<p class="error">⚠️ Error loading incidents: ${error.message}<br><small>Is the backend running on http://localhost:5000?</small></p>`;
    }
}

function displayIncidents(incidents) {
    if (!incidents || incidents.length === 0) {
        incidentsList.innerHTML = '<p style="text-align: center; color: #666;">No incidents reported. Great news!</p>';
        return;
    }

    incidentsList.innerHTML = incidents.map(incident => `
        <div class="list-item">
            <h4>${incident.type || 'Unknown'} - ${incident.tourist?.name || 'Unknown Tourist'}</h4>
            <p><strong>Description:</strong> ${incident.description || 'N/A'}</p>
            <p><strong>Location:</strong> ${(incident.latitude || 0).toFixed(4)}, ${(incident.longitude || 0).toFixed(4)}</p>
            <p><strong>Severity:</strong> ${incident.severity || 'Unknown'}</p>
            <p><strong>Status:</strong> ${incident.isHandled ? 'Handled' : 'Active'}</p>
            <p><strong>Time:</strong> ${new Date(incident.timestamp).toLocaleString()}</p>
            <div class="actions">
                ${!incident.isHandled ? `<button onclick="markHandled(${incident.id})">Mark as Handled</button>` : ''}
                <button onclick="deleteIncident(${incident.id})">Delete</button>
            </div>
        </div>
    `).join('');
}

function showIncidentForm() {
    document.getElementById('incident-form').style.display = 'block';
}

function hideIncidentForm() {
    document.getElementById('incident-form').style.display = 'none';
    document.getElementById('add-incident-form').reset();
}

document.getElementById('add-incident-form').addEventListener('submit', async function(e) {
    e.preventDefault();

    // Validate required fields
    const touristId = document.getElementById('incident-tourist')?.value?.trim();
    const type = document.getElementById('incident-type')?.value?.trim();
    const description = document.getElementById('incident-description')?.value?.trim();
    const severity = document.getElementById('incident-severity')?.value?.trim();
    const lat = document.getElementById('incident-lat')?.value?.trim();
    const lng = document.getElementById('incident-lng')?.value?.trim();

    if (!touristId) {
        showMessage('Please select a tourist', 'error');
        return;
    }
    if (!type) {
        showMessage('Please select incident type', 'error');
        return;
    }
    if (!description) {
        showMessage('Please enter incident description', 'error');
        return;
    }
    if (!severity) {
        showMessage('Please select severity level', 'error');
        return;
    }
    if (!lat || isNaN(lat)) {
        showMessage('Please enter valid latitude', 'error');
        return;
    }
    if (!lng || isNaN(lng)) {
        showMessage('Please enter valid longitude', 'error');
        return;
    }

    const incident = {
        touristId: parseInt(touristId),
        type: type,
        description: description,
        latitude: parseFloat(lat),
        longitude: parseFloat(lng),
        severity: severity
    };

    try {
        const response = await fetch(`${API_BASE_URL}/incidents`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(incident)
        });

        if (response.ok) {
            hideIncidentForm();
            loadIncidents();
            loadDashboard();
            showMessage('Incident reported successfully', 'success');
        } else {
            throw new Error('Failed to report incident');
        }
    } catch (error) {
        console.error('Error reporting incident:', error);
        showMessage(`Error reporting incident. Is backend running at ${API_BASE_URL}?`, 'error');
    }
});

// Danger Zones Management
async function loadDangerZones() {
    try {
        log('Fetching danger zones from API...');
        const response = await fetch(`${API_BASE_URL}/dangerzones`);
        
        if (!response.ok) {
            throw new Error(`API Error: ${response.status} ${response.statusText}`);
        }

        const zones = await response.json();
        log('Danger zones loaded:', zones);
        displayDangerZones(zones || []);

    } catch (error) {
        console.error('❌ Error loading danger zones:', error);
        dangerZonesList.innerHTML = `<p class="error">⚠️ Error loading danger zones: ${error.message}<br><small>Is the backend running on http://localhost:5000?</small></p>`;
    }
}

function displayDangerZones(zones) {
    if (!zones || zones.length === 0) {
        dangerZonesList.innerHTML = '<p style="text-align: center; color: #666;">No danger zones defined. Area is safe!</p>';
        return;
    }

    dangerZonesList.innerHTML = zones.map(zone => `
        <div class="list-item">
            <h4>${zone.name || 'Unknown Zone'}</h4>
            <p><strong>Location:</strong> ${(zone.latitude || 0).toFixed(4)}, ${(zone.longitude || 0).toFixed(4)}</p>
            <p><strong>Radius:</strong> ${zone.radiusMeters || 0} meters</p>
            <p><strong>Risk Level:</strong> ${((zone.riskLevel || 0) * 100).toFixed(1)}%</p>
            <p><strong>Description:</strong> ${zone.description || 'N/A'}</p>
            <p><strong>Reason:</strong> ${zone.reason || 'N/A'}</p>
            <p><strong>Status:</strong> ${zone.isActive ? 'Active' : 'Inactive'}</p>
            <div class="actions">
                <button onclick="editDangerZone(${zone.id})">Edit</button>
                <button onclick="deleteDangerZone(${zone.id})">Delete</button>
            </div>
        </div>
    `).join('');
}

function showDangerZoneForm() {
    document.getElementById('danger-zone-form').style.display = 'block';
}

function hideDangerZoneForm() {
    document.getElementById('danger-zone-form').style.display = 'none';
    document.getElementById('add-danger-zone-form').reset();
}

document.getElementById('add-danger-zone-form').addEventListener('submit', async function(e) {
    e.preventDefault();

    // Validate required fields
    const name = document.getElementById('zone-name')?.value?.trim();
    const lat = document.getElementById('zone-lat')?.value?.trim();
    const lng = document.getElementById('zone-lng')?.value?.trim();
    const radius = document.getElementById('zone-radius')?.value?.trim();
    const risk = document.getElementById('zone-risk')?.value?.trim();
    const description = document.getElementById('zone-description')?.value?.trim();
    const reason = document.getElementById('zone-reason')?.value?.trim();

    if (!name) {
        showMessage('Please enter zone name', 'error');
        return;
    }
    if (!lat || isNaN(lat)) {
        showMessage('Please enter valid latitude', 'error');
        return;
    }
    if (!lng || isNaN(lng)) {
        showMessage('Please enter valid longitude', 'error');
        return;
    }
    if (!radius || isNaN(radius) || radius < 10) {
        showMessage('Please enter valid radius (minimum 10 meters)', 'error');
        return;
    }
    if (!risk || isNaN(risk) || risk < 0 || risk > 1) {
        showMessage('Please enter valid risk level (0.0 to 1.0)', 'error');
        return;
    }
    if (!description) {
        showMessage('Please enter zone description', 'error');
        return;
    }
    if (!reason) {
        showMessage('Please enter reason for danger zone', 'error');
        return;
    }

    const zone = {
        name: name,
        latitude: parseFloat(lat),
        longitude: parseFloat(lng),
        radiusMeters: parseInt(radius),
        riskLevel: parseFloat(risk),
        description: description,
        reason: reason
    };

    try {
        const response = await fetch(`${API_BASE_URL}/dangerzones`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(zone)
        });

        if (response.ok) {
            hideDangerZoneForm();
            loadDangerZones();
            loadDashboard();
            showMessage('Danger zone added successfully', 'success');
        } else {
            throw new Error('Failed to add danger zone');
        }
    } catch (error) {
        console.error('Error adding danger zone:', error);
        showMessage(`Error adding danger zone. Is backend running at ${API_BASE_URL}?`, 'error');
    }
});

// Location Tracking
let trackingInterval;
let currentTouristId;

function startTracking() {
    const touristId = document.getElementById('track-tourist').value;
    if (!touristId) {
        showMessage('Please select a tourist to track', 'error');
        return;
    }

    currentTouristId = touristId;
    trackingInterval = setInterval(updateLocation, 10000); // Update every 10 seconds
    showMessage('Location tracking started', 'success');
}

function stopTracking() {
    if (trackingInterval) {
        clearInterval(trackingInterval);
        trackingInterval = null;
        currentTouristId = null;
        showMessage('Location tracking stopped', 'success');
    }
}

async function updateLocation() {
    if (!currentTouristId) return;

    // In a real app, you'd get location from GPS
    // For demo, we'll use mock coordinates
    const mockLat = 40.7128 + (Math.random() - 0.5) * 0.01;
    const mockLng = -74.0060 + (Math.random() - 0.5) * 0.01;

    try {
        const response = await fetch(`${API_BASE_URL}/location/update`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                touristId: parseInt(currentTouristId),
                latitude: mockLat,
                longitude: mockLng
            })
        });

        if (response.ok) {
            document.getElementById('location-info').innerHTML = `
                <p><strong>Last Update:</strong> ${new Date().toLocaleString()}</p>
                <p><strong>Latitude:</strong> ${mockLat.toFixed(6)}</p>
                <p><strong>Longitude:</strong> ${mockLng.toFixed(6)}</p>
            `;

            // Update location on map
            if (map) {
                updateTouristLocationOnMap(currentTouristId, mockLat, mockLng);

                // Check for danger zones
                checkGeofencing(currentTouristId, mockLat, mockLng);
            }
        }
    } catch (error) {
        console.error('Error updating location:', error);
    }
}

// Utility Functions
function showMessage(message, type) {
    // Remove existing messages
    const existing = document.querySelector('.message-success, .message-error, .message-info');
    if (existing) existing.remove();

    const messageDiv = document.createElement('div');
    messageDiv.className = `message-${type || 'info'}`;
    messageDiv.style.cssText = `
        padding: 15px 20px;
        margin: 10px 0;
        border-radius: 5px;
        font-weight: 500;
        animation: slideIn 0.3s ease-in;
        ${type === 'success' ? 'background-color: #d4edda; color: #155724; border: 1px solid #c3e6cb;' : ''}
        ${type === 'error' ? 'background-color: #f8d7da; color: #721c24; border: 1px solid #f5c6cb;' : ''}
        ${type === 'info' ? 'background-color: #d1ecf1; color: #0c5460; border: 1px solid #bee5eb;' : ''}
    `;
    messageDiv.textContent = message;

    const mainElement = document.querySelector('main') || document.body;
    mainElement.insertBefore(messageDiv, mainElement.firstChild);

    // Add animation with style tag if not already there
    if (!document.querySelector('style[data-message-animation]')) {
        const style = document.createElement('style');
        style.setAttribute('data-message-animation', 'true');
        style.textContent = `
            @keyframes slideIn {
                from {
                    transform: translateY(-20px);
                    opacity: 0;
                }
                to {
                    transform: translateY(0);
                    opacity: 1;
                }
            }
        `;
        document.head.appendChild(style);
    }

    setTimeout(() => {
        if (messageDiv.parentNode) {
            messageDiv.remove();
        }
    }, 5000);
}

// Placeholder functions for edit/delete operations
function editTourist(id) {
    showMessage('Edit functionality coming soon', 'info');
}

function deleteTourist(id) {
    if (confirm('Are you sure you want to delete this tourist?')) {
        fetch(`${API_BASE_URL}/tourists/${id}`, { method: 'DELETE' })
            .then(r => {
                if (r.ok) {
                    showMessage('Tourist deleted successfully', 'success');
                    loadTourists();
                    loadDashboard();
                } else {
                    throw new Error('Failed to delete');
                }
            })
            .catch(e => {
                console.error('Delete error:', e);
                showMessage('Error deleting tourist', 'error');
            });
    }
}

function markHandled(id) {
    if (confirm('Mark this incident as handled?')) {
        fetch(`${API_BASE_URL}/incidents/${id}/mark-handled`, { method: 'PUT' })
            .then(r => {
                if (r.ok) {
                    showMessage('Incident marked as handled', 'success');
                    loadIncidents();
                    loadDashboard();
                } else {
                    throw new Error('Failed to update');
                }
            })
            .catch(e => {
                console.error('Update error:', e);
                showMessage('Error updating incident', 'error');
            });
    }
}

function deleteIncident(id) {
    if (confirm('Are you sure you want to delete this incident?')) {
        fetch(`${API_BASE_URL}/incidents/${id}`, { method: 'DELETE' })
            .then(r => {
                if (r.ok) {
                    showMessage('Incident deleted successfully', 'success');
                    loadIncidents();
                    loadDashboard();
                } else {
                    throw new Error('Failed to delete');
                }
            })
            .catch(e => {
                console.error('Delete error:', e);
                showMessage('Error deleting incident', 'error');
            });
    }
}

function editDangerZone(id) {
    showMessage('Edit functionality coming soon', 'info');
}

function deleteDangerZone(id) {
    if (confirm('Are you sure you want to delete this danger zone?')) {
        fetch(`${API_BASE_URL}/dangerzones/${id}`, { method: 'DELETE' })
            .then(r => {
                if (r.ok) {
                    showMessage('Danger zone deleted successfully', 'success');
                    loadDangerZones();
                    loadDashboard();
                } else {
                    throw new Error('Failed to delete');
                }
            })
            .catch(e => {
                console.error('Delete error:', e);
                showMessage('Error deleting danger zone', 'error');
            });
    }
}

// Google Maps Functions
function initializeMap() {
    // Check if Google Maps API is loaded
    if (typeof google === 'undefined' || typeof google.maps === 'undefined') {
        console.warn('Google Maps API not loaded. Please check your API key.');
        showMapError();
        return;
    }

    // Check if API key is valid (not the placeholder)
    if (GOOGLE_MAPS_API_KEY === 'YOUR_ACTUAL_GOOGLE_MAPS_API_KEY') {
        console.warn('Google Maps API key not configured. Please get an API key from Google Cloud Console.');
        showMapError();
        return;
    }

    try {
        // Default center (can be changed to user's location)
        const defaultCenter = { lat: 40.7128, lng: -74.0060 }; // New York City

        map = new google.maps.Map(document.getElementById('map'), {
            zoom: 12,
            center: defaultCenter,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        });

        // Try to get user's current location
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    const userLocation = {
                        lat: position.coords.latitude,
                        lng: position.coords.longitude
                    };
                    map.setCenter(userLocation);
                    addCurrentLocationMarker(userLocation);
                },
                (error) => {
                    console.log('Geolocation error:', error);
                    addCurrentLocationMarker(defaultCenter);
                }
            );
        }

        // Load danger zones on map
        loadDangerZonesOnMap();
    } catch (error) {
        console.error('Error initializing Google Maps:', error);
        showMapError();
    }
}

function showMapError() {
    const mapContainer = document.getElementById('map');
    mapContainer.innerHTML = `
        <div style="display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; text-align: center; padding: 20px;">
            <div style="font-size: 48px; margin-bottom: 20px;">🗺️</div>
            <h3>Google Maps Not Available</h3>
            <p>To enable maps functionality:</p>
            <ol style="text-align: left; max-width: 400px;">
                <li>Go to <a href="https://console.cloud.google.com/" target="_blank">Google Cloud Console</a></li>
                <li>Create a new project or select existing one</li>
                <li>Enable the "Maps JavaScript API"</li>
                <li>Create credentials (API Key)</li>
                <li>Replace <code>YOUR_ACTUAL_GOOGLE_MAPS_API_KEY</code> in the code with your real API key</li>
            </ol>
            <p><strong>Note:</strong> You can still use all other features of the Tourist Safety System without maps.</p>
        </div>
    `;
    mapContainer.style.backgroundColor = '#f8f9fa';
    mapContainer.style.border = '2px dashed #dee2e6';
function addCurrentLocationMarker(position) {
    if (!map) return;

    if (currentLocationMarker) {
        currentLocationMarker.setMap(null);
    }

    currentLocationMarker = new google.maps.Marker({
        position: position,
        map: map,
        title: 'Your Location',
        icon: {
            url: 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(`
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <circle cx="12" cy="12" r="8" fill="#4285F4" stroke="white" stroke-width="2"/>
                    <circle cx="12" cy="12" r="3" fill="white"/>
                </svg>
            `),
            scaledSize: new google.maps.Size(24, 24)
        }
    });
}

async function loadDangerZonesOnMap() {
    if (!map) return;

    try {
        const zones = await fetch(`${API_BASE_URL}/dangerzones`).then(r => r.json());

        // Clear existing circles
        dangerZoneCircles.forEach(circle => circle.setMap(null));
        dangerZoneCircles = [];

        zones.forEach(zone => {
            const circle = new google.maps.Circle({
                strokeColor: getRiskColor(zone.riskLevel),
                strokeOpacity: 0.8,
                strokeWeight: 2,
                fillColor: getRiskColor(zone.riskLevel),
                fillOpacity: 0.2,
                map: map,
                center: { lat: zone.latitude, lng: zone.longitude },
                radius: zone.radiusMeters
            });

            dangerZoneCircles.push(circle);

            // Add info window
            const infoWindow = new google.maps.InfoWindow({
                content: `
                    <div>
                        <h4>${zone.name}</h4>
                        <p>Risk Level: ${(zone.riskLevel * 100).toFixed(1)}%</p>
                        <p>${zone.description}</p>
                    </div>
                `
            });

            circle.addListener('click', () => {
                infoWindow.setPosition(circle.getCenter());
                infoWindow.open(map);
            });
        });
    } catch (error) {
        console.error('Error loading danger zones on map:', error);
    }
}

function getRiskColor(riskLevel) {
    if (riskLevel >= 0.8) return '#dc3545'; // Red - High risk
    if (riskLevel >= 0.6) return '#fd7e14'; // Orange - Medium-high risk
    if (riskLevel >= 0.4) return '#ffc107'; // Yellow - Medium risk
    return '#28a745'; // Green - Low risk
}

function updateTouristLocationOnMap(touristId, latitude, longitude) {
    if (!map) return;

    // Remove existing markers for this tourist
    markers = markers.filter(marker => {
        if (marker.touristId === touristId) {
            marker.setMap(null);
            return false;
        }
        return true;
    });

    // Add new marker
    const marker = new google.maps.Marker({
        position: { lat: latitude, lng: longitude },
        map: map,
        title: `Tourist ${touristId}`,
        touristId: touristId,
        icon: {
            url: 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(`
                <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <circle cx="10" cy="10" r="8" fill="#FF6B6B" stroke="white" stroke-width="2"/>
                </svg>
            `),
            scaledSize: new google.maps.Size(20, 20)
        }
    });

    markers.push(marker);
    map.setCenter({ lat: latitude, lng: longitude });
}

async function checkGeofencing(touristId, latitude, longitude) {
    if (!map) return;

    try {
        const zones = await fetch(`${API_BASE_URL}/dangerzones`).then(r => r.json());
        const touristPoint = new google.maps.LatLng(latitude, longitude);

        for (const zone of zones) {
            const zoneCenter = new google.maps.LatLng(zone.latitude, zone.longitude);
            const distance = google.maps.geometry.spherical.computeDistanceBetween(touristPoint, zoneCenter);

            if (distance <= zone.radiusMeters) {
                // Tourist is in danger zone - send alert
                await sendGeofencingAlert(touristId, zone);
                break; // Only alert for first danger zone found
            }
        }
    } catch (error) {
        console.error('Error checking geofencing:', error);
    }
}

async function sendGeofencingAlert(touristId, dangerZone) {
    try {
        const response = await fetch(`${API_BASE_URL}/alerts/geofencing`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                touristId: touristId,
                dangerZoneId: dangerZone.id,
                message: `Tourist entered danger zone: ${dangerZone.name} (Risk Level: ${(dangerZone.riskLevel * 100).toFixed(1)}%)`
            })
        });

        if (response.ok) {
            showMessage(`ALERT: Tourist entered danger zone "${dangerZone.name}"!`, 'error');
        }
    } catch (error) {
        console.error('Error sending geofencing alert:', error);
    }
}