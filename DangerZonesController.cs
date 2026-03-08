using Microsoft.AspNetCore.Mvc;
using TouristSafetySystem.Data;
using TouristSafetySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace TouristSafetySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DangerZonesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DangerZonesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/dangerzones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DangerZone>>> GetDangerZones()
        {
            var zones = await _context.DangerZones.Where(dz => dz.IsActive).ToListAsync();
            return Ok(zones);
        }

        // GET: api/dangerzones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DangerZone>> GetDangerZone(int id)
        {
            var zone = await _context.DangerZones.FindAsync(id);

            if (zone == null)
            {
                return NotFound();
            }

            return Ok(zone);
        }

        // POST: api/dangerzones
        [HttpPost]
        public async Task<ActionResult<DangerZone>> CreateDangerZone(DangerZone zone)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            zone.CreatedDate = DateTime.UtcNow;
            zone.IsActive = true;
            _context.DangerZones.Add(zone);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDangerZone), new { id = zone.Id }, zone);
        }

        // PUT: api/dangerzones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDangerZone(int id, DangerZone zone)
        {
            if (id != zone.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            zone.LastUpdated = DateTime.UtcNow;
            _context.Entry(zone).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DangerZoneExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/dangerzones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDangerZone(int id)
        {
            var zone = await _context.DangerZones.FindAsync(id);
            if (zone == null)
            {
                return NotFound();
            }

            // Soft delete - mark as inactive
            zone.IsActive = false;
            zone.LastUpdated = DateTime.UtcNow;
            _context.Entry(zone).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DangerZoneExists(int id)
        {
            return _context.DangerZones.Any(e => e.Id == id);
        }
    }
}