"""
Flask API server for the Risk Prediction AI Model.
This server provides REST endpoints for risk prediction and incident analysis.
"""

from flask import Flask, request, jsonify
from risk_model import RiskPredictionModel, RiskAnalyzer
import logging
import os
from datetime import datetime

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Initialize Flask app
app = Flask(__name__)
app.config['JSON_SORT_KEYS'] = False

# Initialize the risk prediction model
try:
    risk_model = RiskPredictionModel()
    logger.info("Risk Prediction Model initialized successfully")
except Exception as e:
    logger.error(f"Failed to initialize Risk Prediction Model: {e}")
    risk_model = None


@app.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({
        "status": "healthy",
        "model_loaded": risk_model is not None,
        "timestamp": datetime.now().isoformat()
    }), 200


@app.route('/api/predict-risk', methods=['POST'])
def predict_risk():
    """
    Predict risk level for a tourist at a given location.
    
    Expected JSON payload:
    {
        "tourist": {
            "id": 1,
            "age": 35,
            "first_time_visitor": true,
            "days_in_country": 3
        },
        "location": {
            "latitude": 40.7128,
            "longitude": -74.0060
        },
        "recent_incidents": [
            {"type": "theft", "timestamp": "2026-03-07T10:30:00"},
            {"type": "accident", "timestamp": "2026-03-07T14:20:00"}
        ]
    }
    """
    try:
        data = request.get_json()
        
        if not data:
            return jsonify({"error": "No JSON data provided"}), 400
        
        if risk_model is None:
            return jsonify({"error": "AI model not available"}), 503
        
        # Extract data
        tourist_data = data.get('tourist', {})
        location_data = data.get('location', {})
        incident_data = data.get('recent_incidents', [])
        
        # Validate required fields
        if not location_data.get('latitude') or not location_data.get('longitude'):
            return jsonify({"error": "Location coordinates are required"}), 400
        
        # Extract features and predict
        features = risk_model.extract_features(tourist_data, location_data, incident_data)
        prediction = risk_model.predict_risk(features)
        
        # Add geofencing analysis
        prediction['danger_zones'] = identify_danger_zones(
            location_data['latitude'],
            location_data['longitude']
        )
        
        return jsonify({
            "success": True,
            "data": prediction
        }), 200
    
    except Exception as e:
        logger.error(f"Error in predict_risk: {e}")
        return jsonify({"error": str(e)}), 500


@app.route('/api/analyze-incidents', methods=['POST'])
def analyze_incidents():
    """
    Analyze incident patterns and trends.
    
    Expected JSON payload:
    {
        "incidents": [
            {"type": "theft", "timestamp": "2026-03-07T10:30:00", "location": "downtown"},
            ...
        ]
    }
    """
    try:
        data = request.get_json()
        incidents = data.get('incidents', [])
        
        if not incidents:
            return jsonify({"error": "No incidents provided"}), 400
        
        analysis = RiskAnalyzer.analyze_incident_patterns(incidents)
        
        return jsonify({
            "success": True,
            "data": analysis
        }), 200
    
    except Exception as e:
        logger.error(f"Error in analyze_incidents: {e}")
        return jsonify({"error": str(e)}), 500


@app.route('/api/danger-zones', methods=['GET'])
def get_danger_zones():
    """Get all danger zones"""
    if risk_model is None:
        return jsonify({"error": "AI model not available"}), 503
    
    zones = [
        {
            "id": i,
            "latitude": zone['lat'],
            "longitude": zone['lng'],
            "radius": zone['radius'],
            "risk_level": zone['risk_level'],
            "description": f"Danger Zone {i+1}"
        }
        for i, zone in enumerate(risk_model.danger_zones)
    ]
    
    return jsonify({
        "success": True,
        "data": zones
    }), 200


@app.route('/api/check-geofence', methods=['POST'])
def check_geofence():
    """
    Check if a location is within a danger zone (geofence).
    
    Expected JSON payload:
    {
        "latitude": 40.7128,
        "longitude": -74.0060
    }
    """
    try:
        data = request.get_json()
        lat = data.get('latitude')
        lng = data.get('longitude')
        
        if lat is None or lng is None:
            return jsonify({"error": "Location coordinates are required"}), 400
        
        if risk_model is None:
            return jsonify({"error": "AI model not available"}), 503
        
        # Check proximity to danger zones
        in_danger_zone = False
        closest_zone = None
        min_distance = float('inf')
        
        for zone in risk_model.danger_zones:
            distance = risk_model._haversine_distance(lat, lng, zone['lat'], zone['lng'])
            
            if distance < zone['radius']:
                in_danger_zone = True
                closest_zone = zone
                break
            
            if distance < min_distance:
                min_distance = distance
                closest_zone = zone
        
        return jsonify({
            "success": True,
            "data": {
                "in_danger_zone": in_danger_zone,
                "closest_zone": {
                    "latitude": closest_zone['lat'],
                    "longitude": closest_zone['lng'],
                    "radius": closest_zone['radius'],
                    "distance": min_distance if not in_danger_zone else 0
                } if closest_zone else None,
                "alert_message": "WARNING: You are in a danger zone!" if in_danger_zone else "You are safe"
            }
        }), 200
    
    except Exception as e:
        logger.error(f"Error in check_geofence: {e}")
        return jsonify({"error": str(e)}), 500


def identify_danger_zones(lat, lng, radius=5000):
    """
    Identify danger zones near a given location.
    
    Args:
        lat: Latitude
        lng: Longitude
        radius: Search radius in meters
    
    Returns:
        List of nearby danger zones
    """
    if risk_model is None:
        return []
    
    nearby_zones = []
    for zone in risk_model.danger_zones:
        distance = risk_model._haversine_distance(lat, lng, zone['lat'], zone['lng'])
        if distance <= radius:
            nearby_zones.append({
                "latitude": zone['lat'],
                "longitude": zone['lng'],
                "radius": zone['radius'],
                "risk_level": zone['risk_level'],
                "distance": distance
            })
    
    return sorted(nearby_zones, key=lambda x: x['distance'])


@app.errorhandler(404)
def not_found(error):
    return jsonify({"error": "Endpoint not found"}), 404


@app.errorhandler(500)
def internal_error(error):
    return jsonify({"error": "Internal server error"}), 500


if __name__ == '__main__':
    # Get configuration from environment variables
    host = os.getenv('AI_SERVER_HOST', '127.0.0.1')
    port = int(os.getenv('AI_SERVER_PORT', 5000))
    debug = os.getenv('AI_SERVER_DEBUG', 'False') == 'True'
    
    logger.info(f"Starting AI Server on {host}:{port}")
    app.run(host=host, port=port, debug=debug)
