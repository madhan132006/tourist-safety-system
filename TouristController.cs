using Microsoft.AspNetCore.Mvc;
using TouristSafetySystem.Data;
using TouristSafetySystem.Models;
using TouristSafetySystem.Services;
using Microsoft.EntityFrameworkCore;

namespace TouristSafetySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TouristController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BlockchainService _blockchain;

        public TouristController(ApplicationDbContext context, BlockchainService blockchain)
        {
            _context = context;
            _blockchain = blockchain;
        }

        // GET: api/tourists
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tourist>>> GetTourists()
        {
            var tourists = await _context.Tourists.ToListAsync();
            return Ok(tourists);
        }

        // GET: api/tourists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tourist>> GetTourist(int id)
        {
            var tourist = await _context.Tourists.FindAsync(id);

            if (tourist == null)
            {
                return NotFound();
            }

            return Ok(tourist);
        }

        // POST: api/tourists
        [HttpPost]
        public async Task<ActionResult<Tourist>> CreateTourist(Tourist tourist)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Tourists.Add(tourist);
            await _context.SaveChangesAsync();

            // create blockchain identity
            try
            {
                var address = await _blockchain.CreateIdentityAsync(tourist.Name, tourist.Email);
                tourist.BlockchainAddress = address;
                _context.Update(tourist);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't fail the tourist creation
                Console.WriteLine($"Blockchain error: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetTourist), new { id = tourist.Id }, tourist);
        }

        // PUT: api/tourists/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTourist(int id, Tourist tourist)
        {
            if (id != tourist.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(tourist).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TouristExists(id))
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

        // DELETE: api/tourists/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTourist(int id)
        {
            var tourist = await _context.Tourists.FindAsync(id);
            if (tourist == null)
            {
                return NotFound();
            }

            _context.Tourists.Remove(tourist);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TouristExists(int id)
        {
            return _context.Tourists.Any(e => e.Id == id);
        }
    }
}