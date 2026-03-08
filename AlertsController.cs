using Microsoft.AspNetCore.Mvc;
using TouristSafetySystem.Services;

namespace TouristSafetySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public AlertsController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("geofencing")]
        public async Task<IActionResult> SendGeofencingAlert([FromBody] GeofencingAlertRequest request)
        {
            try
            {
                await _notificationService.SendGeofencingAlertAsync(
                    request.TouristId,
                    request.DangerZoneId,
                    request.Message
                );

                return Ok(new { message = "Geofencing alert sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Failed to send geofencing alert: {ex.Message}" });
            }
        }
    }

    public class GeofencingAlertRequest
    {
        public int TouristId { get; set; }
        public int DangerZoneId { get; set; }
        public string Message { get; set; }
    }
}