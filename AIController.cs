using Microsoft.AspNetCore.Mvc;
using TouristSafetySystem.Services;

namespace TouristSafetySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly AIService _aiService;

        public AIController(AIService aiService)
        {
            _aiService = aiService;
        }

        // POST: api/ai/predict
        [HttpPost("predict")]
        public async Task<IActionResult> PredictRisk([FromBody] AIService.AIRiskRequest request)
        {
            try
            {
                var riskLevel = await _aiService.PredictRiskAsync(request);
                return Ok(new { riskLevel = riskLevel });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "AI prediction failed", details = ex.Message });
            }
        }

        // POST: api/ai/analyze-incident
        [HttpPost("analyze-incident")]
        public async Task<IActionResult> AnalyzeIncident([FromBody] AIService.AIIncidentRequest request)
        {
            try
            {
                var analysis = await _aiService.AnalyzeIncidentAsync(request);
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Incident analysis failed", details = ex.Message });
            }
        }
    }
}