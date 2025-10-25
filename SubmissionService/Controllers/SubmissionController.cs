using Microsoft.AspNetCore.Mvc;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Service;

namespace SubmissionService.Controllers
{
    [ApiController]
    [Route("api/Submission")]
    public class SubmissionController : Controller
    {
        private readonly SubmissionManager _submissionManager;
        public SubmissionController(SubmissionManager sub)
        {
            _submissionManager = sub;
        }
        [HttpPost("Submit")]
        public async Task<IActionResult> Submit(Request request)
        {

            try
            {
                bool success = await _submissionManager.Submit(request);
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


        [HttpPost("RunCode")]
        public async Task<IActionResult> RunCode([FromBody] Request request)
        {

            try
            {
                Console.WriteLine("chjecakjdjas"+request.Stdin);
                var result = await _submissionManager.RunCode(request.SourceCode??"",request.LanguageId,request.Stdin);
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
