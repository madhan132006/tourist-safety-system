using Microsoft.AspNetCore.Mvc;
using TouristSafetySystem.Data;
using TouristSafetySystem.Models;
using TouristSafetySystem.Services;
using Microsoft.EntityFrameworkCore;

namespace TouristSafetySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notifier;
        private readonly AIService _aiService;

        public IncidentsController(ApplicationDbContext context, NotificationService notifier, AIService aiService)
        {
            _context = context;
            _notifier = notifier;
            _aiService = aiService;
        }

        // GET: api/incidents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Incident>>> GetIncidents()
        {
            var incidents = await _context.Incidents.Include(i => i.Tourist).ToListAsync();
            return Ok(incidents);
        }

        // GET: api/incidents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Incident>> GetIncident(int id)
        {
            var incident = await _context.Incidents.Include(i => i.Tourist).FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
            {
                return NotFound();
            }

            return Ok(incident);
        }

        // POST: api/incidents
        [HttpPost]
        public async Task<ActionResult<Incident>> CreateIncident(Incident incident)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            incident.Timestamp = DateTime.UtcNow;
            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();

            // Analyze incident with AI
            try
            {
                var analysisRequest = new AIService.AIIncidentRequest
                {
                    Type = incident.Type,
                    Description = incident.Description,
                    Latitude = incident.Latitude,
                    Longitude = incident.Longitude,
                    Timestamp = incident.Timestamp
                };

                var analysis = await _aiService.AnalyzeIncidentAsync(analysisRequest);
                // Could store analysis results in the database
                Console.WriteLine($"AI Analysis: Severity {analysis.Severity}, Confidence {analysis.Confidence}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Analysis failed: {ex.Message}");
            }

            // Send notification
            try
            {
                var tourist = await _context.Tourists.FindAsync(incident.TouristId);
                if (tourist != null && !string.IsNullOrEmpty(tourist.PhoneNumber))
                {
                    await _notifier.SendSmsAsync(
                        tourist.PhoneNumber,
                        $"Incident reported: {incident.Type} at {incident.LocationName ?? "your location"}. Stay safe!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification failed: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetIncident), new { id = incident.Id }, incident);
        }

        // PUT: api/incidents/5/handle
        [HttpPut("{id}/handle")]
        public async Task<IActionResult> MarkIncidentHandled(int id)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null)
            {
                return NotFound();
            }

            incident.IsHandled = true;
            incident.ResolvedDate = DateTime.UtcNow;
            _context.Entry(incident).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/incidents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null)
            {
                return NotFound();
            }

            _context.Incidents.Remove(incident);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}