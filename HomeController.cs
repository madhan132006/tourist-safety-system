using Microsoft.AspNetCore.Mvc;
using TouristSafetySystem.Data;
using Microsoft.EntityFrameworkCore;

namespace TouristSafetySystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLocation(int touristId, double lat, double lng)
        {
            var tourist = await _context.Tourists.FindAsync(touristId);
            if (tourist != null)
            {
                tourist.Latitude = lat;
                tourist.Longitude = lng;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}