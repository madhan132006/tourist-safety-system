using Microsoft.AspNetCore.Mvc;
using TouristSafetySystem.Data;
using TouristSafetySystem.Models;
using TouristSafetySystem.Services;
using Microsoft.EntityFrameworkCore;

namespace TouristSafetySystem.Controllers
{
    public class IncidentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notifier;
        private readonly AIService _aiService;

        public IncidentController(ApplicationDbContext context, NotificationService notifier, AIService aiService)
        {
            _context = context;
            _notifier = notifier;
            _aiService = aiService;
        }

        public async Task<IActionResult> Index()
        {
            var incidents = await _context.Incidents.Include(i => i.Tourist).ToListAsync();
            return View(incidents);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Tourists = await _context.Tourists.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Incident incident)
        {
            if (ModelState.IsValid)
            {
                incident.Timestamp = DateTime.UtcNow;
                _context.Incidents.Add(incident);
                await _context.SaveChangesAsync();

                // send a notification to admin or emergency contact (example)
                // _notifier.SendSmsAsync("+19876543210", $"New incident: {incident.Type}");

                return RedirectToAction(nameof(Index));
            }
            return View(incident);
        }

        [HttpPost]
        public async Task<IActionResult> MarkHandled(int id)
        {
            var inc = await _context.Incidents.FindAsync(id);
            if (inc != null)
            {
                inc.IsHandled = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}