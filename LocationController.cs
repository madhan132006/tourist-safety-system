using Microsoft.AspNetCore.Mvc;
using TouristSafetySystem.Services;
using TouristSafetySystem.Models;
using TouristSafetySystem.Data;
using Microsoft.EntityFrameworkCore;

namespace TouristSafetySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly LocationTrackingService _locationService;
        private readonly ApplicationDbContext _context;
        private readonly MapsService _mapsService;
        private readonly AIService _aiService;

        public LocationController(
            LocationTrackingService locationService,
            ApplicationDbContext context,
            MapsService mapsService,
            AIService aiService)
        {
            _locationService = locationService;
            _context = context;
            _mapsService = mapsService;
            _aiService = aiService;
        }

        // POST: api/location/update
        [HttpPost("update")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _locationService.UpdateTouristLocationAsync(
                    request.TouristId,
                    request.Latitude,
                    request.Longitude);

                return Ok(new { message = "Location updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update location", details = ex.Message });
            }
        }

        // GET: api/location/history/{touristId}
        [HttpGet("history/{touristId}")]
        public async Task<IActionResult> GetLocationHistory(int touristId, [FromQuery] DateTime? fromDate)
        {
            try
            {
                var history = await _locationService.GetTouristLocationHistoryAsync(touristId, fromDate);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve location history", details = ex.Message });
            }
        }

        // POST: api/location/risk-assessment
        [HttpPost("risk-assessment")]
        public async Task<IActionResult> AssessRisk([FromBody] RiskAssessmentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tourist = await _context.Tourists.FindAsync(request.TouristId);
                if (tourist == null)
                    return NotFound(new { error = "Tourist not found" });

                var riskRequest = new AIService.AIRiskRequest
                {
                    Tourist = new AIService.TouristData
                    {
                        Id = tourist.Id,
                        Age = tourist.Age,
                        FirstTimeVisitor = tourist.FirstTimeVisitor,
                        DaysInCountry = request.DaysInCountry
                    },
                    Location = new AIService.LocationData
                    {
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        Address = await _mapsService.ReverseGeocodeAsync(request.Latitude, request.Longitude),
                        ProximityToDangerZone = await GetClosestDangerZoneDistanceAsync(request.Latitude, request.Longitude)
                    },
                    Weather = request.Weather ?? new AIService.WeatherData
                    {
                        Condition = "Unknown",
                        Temperature = 20,
                        Humidity = 50
                    }
                };

                var riskLevel = await _aiService.PredictRiskAsync(riskRequest);

                return Ok(new
                {
                    riskLevel = riskLevel,
                    riskCategory = GetRiskCategory(riskLevel),
                    recommendations = GetRecommendations(riskLevel)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Risk assessment failed", details = ex.Message });
            }
        }

        // GET: api/location/geocode
        [HttpGet("geocode")]
        public async Task<IActionResult> GeocodeAddress([FromQuery] string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return BadRequest(new { error = "Address is required" });

            try
            {
                var coordinates = await _mapsService.GeocodeAddressAsync(address);
                if (coordinates == null)
                    return NotFound(new { error = "Address not found" });

                return Ok(new
                {
                    latitude = coordinates.Value.lat,
                    longitude = coordinates.Value.lng,
                    address = address
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Geocoding failed", details = ex.Message });
            }
        }

        // GET: api/location/maps-config
        [HttpGet("maps-config")]
        public IActionResult GetMapsConfig()
        {
            // Return Google Maps API key for frontend
            var apiKey = _mapsService.GetApiKey(); // Need to add this method
            return Ok(new { googleMapsApiKey = apiKey });
        }

        // POST: api/location/geolocate
        [HttpPost("geolocate")]
        public async Task<IActionResult> Geolocate([FromBody] MapsService.GeolocationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var location = await _mapsService.GetGeolocationAsync(request);
                if (location == null)
                    return NotFound(new { error = "Location could not be determined" });

                return Ok(new
                {
                    latitude = location.Value.lat,
                    longitude = location.Value.lng
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Geolocation failed", details = ex.Message });
            }
        }

        // GET: api/location/directions
        [HttpGet("directions")]
        public async Task<IActionResult> GetDirections([FromQuery] double originLat, [FromQuery] double originLng, [FromQuery] double destLat, [FromQuery] double destLng)
        {
            try
            {
                var directions = await _mapsService.GetDirectionsAsync(originLat, originLng, destLat, destLng);
                if (directions == null || directions.Status != "OK")
                    return NotFound(new { error = "Directions not found" });

                return Ok(directions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Directions request failed", details = ex.Message });
            }
        }

        // POST: api/location/reverse-geocode
        [HttpPost("reverse-geocode")]
        [HttpGet("reverse-geocode")]
        public async Task<IActionResult> ReverseGeocode([FromQuery] double latitude, [FromQuery] double longitude)
        {
            try
            {
                var address = await _mapsService.ReverseGeocodeAsync(latitude, longitude);
                if (address == null)
                    return NotFound(new { error = "Location not found" });

                return Ok(new
                {
                    latitude = latitude,
                    longitude = longitude,
                    address = address
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Reverse geocoding failed", details = ex.Message });
            }
        }

        private async Task<double> GetClosestDangerZoneDistanceAsync(double lat, double lng)
        {
            var dangerZones = await _context.DangerZones
                .Where(dz => dz.IsActive)
                .ToListAsync();

            if (!dangerZones.Any()) return double.MaxValue;

            var minDistance = double.MaxValue;
            foreach (var zone in dangerZones)
            {
                var distance = _mapsService.GetDistance(lat, lng, zone.Latitude, zone.Longitude);
                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }

        private string GetRiskCategory(double riskLevel)
        {
            if (riskLevel < 0.3) return "Low";
            if (riskLevel < 0.7) return "Medium";
            return "High";
        }

        private List<string> GetRecommendations(double riskLevel)
        {
            var recommendations = new List<string>();

            if (riskLevel >= 0.7)
            {
                recommendations.Add("Avoid traveling alone at night");
                recommendations.Add("Stay in well-lit, populated areas");
                recommendations.Add("Keep emergency contacts updated");
                recommendations.Add("Consider local transportation services");
            }
            else if (riskLevel >= 0.3)
            {
                recommendations.Add("Be aware of your surroundings");
                recommendations.Add("Keep valuables secure");
                recommendations.Add("Have local emergency numbers saved");
            }
            else
            {
                recommendations.Add("Enjoy your trip safely");
            }

            return recommendations;
        }
    }

    // Request/Response models
    public class LocationUpdateRequest
    {
        public int TouristId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class RiskAssessmentRequest
    {
        public int TouristId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int DaysInCountry { get; set; }
        public AIService.WeatherData? Weather { get; set; }
    }
}