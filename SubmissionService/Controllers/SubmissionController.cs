using Microsoft.AspNetCore.Mvc;
using SubmissionService.Application;
using SubmissionService.Application.DTOs;

namespace SubmissionService.Controllers
{
    [ApiController]
    [Route("api/Submission")]
    public class SubmissionController : Controller
    {
        private readonly Sub _sub;
        public SubmissionController(Sub sub)
        {
            _sub = sub;
        }
        [HttpPost("Submit")]
        public async Task<IActionResult> Submit(Request request)
        {

            try
            {
                bool success = await _sub.Submit(request);
                if (!success)
                {
                    // Submit thất bại, ví dụ do Judge0 trả lỗi
                    return BadRequest(new { message = "Submission failed" });
                }

                return Ok(new { message = "Submission successful" });
            }
            catch (HttpRequestException ex)
            {
                // Lỗi network / HTTP
                return StatusCode(503, new { message = "Cannot reach Judge0 API", detail = ex.Message });
            }
            catch (Exception ex)
            {
                // Các lỗi khác
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }


        [HttpPost("SubmitAndShow")]
        public async Task<IActionResult> SubmitAndShow([FromBody] Request request)
        {

            try
            {
                var result = await _sub.SubmitWithResult(request);
                if (result == null) { return BadRequest(new { message = "Submission failed" }); }

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                // Lỗi network / HTTP
                return StatusCode(503, new { message = "Cannot reach Judge0 API", detail = ex.Message });
            }
            catch (Exception ex)
            {
                // Các lỗi khác
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }
    }
}
