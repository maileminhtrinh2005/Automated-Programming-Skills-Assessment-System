using FeedbackService.Application.Constants;
using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestcaseFeedbackController : ControllerBase
    {
        private readonly ITestcaseFeedbackGenerator _gen;

        public TestcaseFeedbackController(ITestcaseFeedbackGenerator gen)
        {
            _gen = gen;
        }

        // CHẤM CHI TIẾT
        [Authorize(Roles = "Lecturer, Admin")]
        [HttpPost("testcasesubmit")]
        public async Task<IActionResult> Submit([FromBody] TestcaseFeedbackRequestDto req, CancellationToken ct)
        {
            try
            {
                if (req.TestResults == null || req.TestResults.Count == 0)
                    return BadRequest(new { error = "Thiếu TestResults để chấm chi tiết." });


                var result = await _gen.GenerateAsync(req, Prompt.PerTestcaseFeedback, ct);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
