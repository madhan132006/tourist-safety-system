using Microsoft.AspNetCore.Mvc;
using TouristSafetySystem.Services;

namespace TouristSafetySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlockchainController : ControllerBase
    {
        private readonly BlockchainService _blockchainService;

        public BlockchainController(BlockchainService blockchainService)
        {
            _blockchainService = blockchainService;
        }

        // POST: api/blockchain/create-identity
        [HttpPost("create-identity")]
        public async Task<IActionResult> CreateIdentity([FromBody] CreateIdentityRequest request)
        {
            try
            {
                var address = await _blockchainService.CreateIdentityAsync(request.Name, request.Email);
                return Ok(new { blockchainAddress = address });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create blockchain identity", details = ex.Message });
            }
        }

        // GET: api/blockchain/verify-identity/{address}
        [HttpGet("verify-identity/{address}")]
        public async Task<IActionResult> VerifyIdentity(string address)
        {
            try
            {
                var isValid = await _blockchainService.VerifyIdentityAsync(address);
                return Ok(new { isValid = isValid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to verify identity", details = ex.Message });
            }
        }

        // GET: api/blockchain/identity-metadata/{address}
        [HttpGet("identity-metadata/{address}")]
        public async Task<IActionResult> GetIdentityMetadata(string address)
        {
            try
            {
                var metadata = await _blockchainService.GetIdentityMetadataAsync(address);
                return Ok(new { metadata = metadata });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve metadata", details = ex.Message });
            }
        }
    }

    public class CreateIdentityRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}