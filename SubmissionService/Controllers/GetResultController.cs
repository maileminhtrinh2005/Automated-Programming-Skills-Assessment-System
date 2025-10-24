using Microsoft.AspNetCore.Mvc;
using SubmissionService.Application.Service;

namespace SubmissionService.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class GetResultController : Controller
    {
        private readonly SubmissionControl _sub;
        public GetResultController(SubmissionControl sub)
        {
            _sub = sub;
        }

        [HttpGet("getAllSubmissions")]
        public  async Task<IActionResult> GetSubmissionsByID(int studentId)
        {
            var submissions = await _sub.GetAllYourSubmissions(studentId);
            if (submissions == null || !submissions.Any()) { return NotFound(); }

            return Ok(submissions);
        }

        [HttpGet("getYourResults")]
        public async Task<IActionResult> GetYourResults(int studentId, int assignmentId)
        {
            var results = await _sub.GetResults(studentId, assignmentId);
            if (!results.Any()|| results==null) { return NotFound(); }
            return Ok(results);
        }

    }
}
