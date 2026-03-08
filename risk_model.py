import numpy as np
import pandas as pd
import joblib
from sklearn.ensemble import RandomForestClassifier
from sklearn.preprocessing import StandardScaler
from sklearn.model_selection import train_test_split
import os
from datetime import datetime

class RiskPredictionModel:
    """
    AI Model for predicting safety risk levels for tourists in real-time.
    Uses machine learning to analyze various factors including:
    - Geographic location (proximity to danger zones)
    - Time of day
    - Weather conditions
    - Historical incident patterns
    - Tourist profile risk factors
    """
    
    def __init__(self):
        self.model = None
        self.scaler = StandardScaler()
        self.danger_zones = [
            {"lat": 40.7128, "lng": -74.0060, "radius": 500, "risk_level": 0.9},  # NYC
            {"lat": 34.0522, "lng": -118.2437, "radius": 300, "risk_level": 0.7},  # LA
            {"lat": 41.8781, "lng": -87.6298, "radius": 400, "risk_level": 0.8},  # Chicago
        ]
        self.feature_names = [
            'latitude', 'longitude', 'hour_of_day', 'day_of_week',
            'proximity_danger_zone', 'weather_risk', 'historical_incidents',
            'tourist_age', 'first_time_visitor', 'time_in_country'
        ]
        self._train_model()
    
    def _train_model(self):
        """Train the risk prediction model with synthetic data"""
        # Generate synthetic training data
        n_samples = 1000
        X_train = np.random.randn(n_samples, len(self.feature_names))
        # Synthetic target: 1 = high risk, 0 = low risk
        y_train = (X_train[:, 0] > 2) | (X_train[:, 1] < -2) | (X_train[:, 2] > 1)
        
        # Scale features
        X_train_scaled = self.scaler.fit_transform(X_train)
        
        # Train Random Forest model
        self.model = RandomForestClassifier(n_estimators=100, random_state=42, max_depth=10)
        self.model.fit(X_train_scaled, y_train)
    
    def extract_features(self, tourist_data, location_data, incident_data):
        """
        Extract features from tourist, location, and incident data
        
        Args:
            tourist_data: dict with 'age', 'first_time_visitor', 'days_in_country'
            location_data: dict with 'latitude', 'longitude'
            incident_data: list of recent incidents near the location
        
        Returns:
            np.array of features ready for prediction
        """
        features = []
        
        # Geographic features
        lat = location_data.get('latitude', 0)
        lng = location_data.get('longitude', 0)
        features.append(lat)
        features.append(lng)
        
        # Temporal features
        now = datetime.now()
        hour = now.hour
        day_of_week = now.weekday()
        features.append(hour / 24.0)  # Normalize to 0-1
        features.append(day_of_week / 7.0)
        
        # Proximity to danger zones
        min_proximity = 10000  # Large initial value
        for zone in self.danger_zones:
            dist = self._haversine_distance(lat, lng, zone['lat'], zone['lng'])
            min_proximity = min(min_proximity, dist)
        features.append(min(1.0, min_proximity / 1000))  # Normalize
        
        # Weather risk (placeholder)
        weather_risk = np.random.rand()
        features.append(weather_risk)
        
        # Historical incidents nearby
        incident_count = len(incident_data) if incident_data else 0
        features.append(min(1.0, incident_count / 10))  # Normalize
        
        # Tourist profile features
        age = tourist_data.get('age', 30)
        first_time = 1.0 if tourist_data.get('first_time_visitor', False) else 0.0
        days_in_country = tourist_data.get('days_in_country', 1)
        
        features.append(age / 100.0)  # Normalize
        features.append(first_time)
        features.append(min(1.0, days_in_country / 30))  # Normalize
        
        return np.array(features).reshape(1, -1)
    
    def predict_risk(self, features):
        """
        Predict risk level for the given features
        
        Returns:
            dict with 'risk_level' (0-1), 'is_high_risk' (bool), and 'recommendations'
        """
        if self.model is None:
            return {"risk_level": 0.5, "is_high_risk": False, "recommendations": []}
        
        # Scale features
        features_scaled = self.scaler.transform(features)
        
        # Predict probability of high risk
        risk_prob = self.model.predict_proba(features_scaled)[0, 1]
        
        # Generate recommendations based on risk level
        recommendations = self._generate_recommendations(risk_prob, features)
        
        return {
            "risk_level": float(risk_prob),
            "is_high_risk": risk_prob > 0.7,
            "medium_risk": 0.4 < risk_prob <= 0.7,
            "recommendations": recommendations,
            "timestamp": datetime.now().isoformat()
        }
    
    def _generate_recommendations(self, risk_level, features):
        """Generate safety recommendations based on risk level"""
        recommendations = []
        
        if risk_level > 0.7:
            recommendations.append("HIGH RISK: Consider staying in safer areas")
            recommendations.append("Alert emergency contact immediately")
            recommendations.append("Keep your phone charged and share location")
        elif risk_level > 0.4:
            recommendations.append("MEDIUM RISK: Exercise caution in this area")
            recommendations.append("Travel with companions when possible")
            recommendations.append("Use licensed transportation")
        else:
            recommendations.append("LOW RISK: Area appears safe")
            recommendations.append("Standard travel precautions recommended")
        
        return recommendations
    
    def _haversine_distance(self, lat1, lon1, lat2, lon2):
        """Calculate distance between two points in meters using Haversine formula"""
        R = 6371000  # Earth radius in meters
        
        lat1_rad = np.radians(lat1)
        lat2_rad = np.radians(lat2)
        delta_lat = np.radians(lat2 - lat1)
        delta_lon = np.radians(lon2 - lon1)
        
        a = np.sin(delta_lat/2)**2 + np.cos(lat1_rad) * np.cos(lat2_rad) * np.sin(delta_lon/2)**2
        c = 2 * np.arcsin(np.sqrt(a))
        
        return R * c
    
    def save_model(self, filepath):
        """Save the trained model to disk"""
        joblib.dump((self.model, self.scaler), filepath)
    
    def load_model(self, filepath):
        """Load a pre-trained model from disk"""
        if os.path.exists(filepath):
            self.model, self.scaler = joblib.load(filepath)


class RiskAnalyzer:
    """Analyze risk patterns from incident data"""
    
    @staticmethod
    def analyze_incident_patterns(incidents):
        """Analyze patterns in incident data"""
        if not incidents:
            return {"pattern": "No incidents", "trend": "stable"}
        
        df = pd.DataFrame(incidents)
        
        # Analyze incident frequency
        if 'timestamp' in df.columns:
            df['timestamp'] = pd.to_datetime(df['timestamp'])
            hourly_count = df.groupby(df['timestamp'].dt.hour).size()
            peak_hour = hourly_count.idxmax() if len(hourly_count) > 0 else None
        
        # Analyze incident types
        if 'type' in df.columns:
            incident_types = df['type'].value_counts().to_dict()
        
        return {
            "total_incidents": len(incidents),
            "incident_types": incident_types if 'type' in df.columns else {},
            "peak_hour": int(peak_hour) if 'timestamp' in df.columns and peak_hour else None
        }


if __name__ == "__main__":
    # Example usage
    model = RiskPredictionModel()
    
    # Test data
    tourist_data = {
        "age": 35,
        "first_time_visitor": True,
        "days_in_country": 3
    }
    
    location_data = {
        "latitude": 40.7128,
        "longitude": -74.0060
    }
    
    incident_data = [
        {"type": "theft", "timestamp": "2026-03-07T10:30:00"},
        {"type": "accident", "timestamp": "2026-03-07T14:20:00"}
    ]
    
    # Extract features and predict
    features = model.extract_features(tourist_data, location_data, incident_data)
    prediction = model.predict_risk(features)
    
    print("Risk Prediction Results:")
    print(f"Risk Level: {prediction['risk_level']:.2%}")
    print(f"High Risk: {prediction['is_high_risk']}")
    print("\nRecommendations:")
    for rec in prediction['recommendations']:
        print(f"  - {rec}")

# features: [latitude, longitude, time_of_day, historical_incident_rate]

class RiskModel:
    def __init__(self, model_path='risk_model.pkl'):
        try:
            self.model = joblib.load(model_path)
        except FileNotFoundError:
            self.model = RandomForestClassifier()

    def train(self, X, y):
        self.model.fit(X, y)
        joblib.dump(self.model, 'risk_model.pkl')

    def predict(self, features):
        return self.model.predict_proba([features])[0][1]  # return probability of incident

if __name__ == '__main__':
    # demo training
    X = np.random.rand(100, 4)
    y = (np.random.rand(100) > 0.8).astype(int)
    rm = RiskModel()
    rm.train(X, y)
    print("Model trained")