using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubmissionService.Application.Service;
using System.Security.Claims;

namespace SubmissionService.Controllers
{
    [ApiController]
    [Route("api/Result")]
    public class ResultController : Controller
    {
        private readonly SubmissionManager _submissionManager;
        private readonly ResultManager _resultManager;
        public ResultController(SubmissionManager sub,ResultManager resultManager)
        {
            _submissionManager = sub;
            _resultManager = resultManager;
        }

        [Authorize(Roles ="Admin,Lecturer")]
        [HttpGet("GetSubmissions/{id}")] // id hoc sinh //1 
        public async Task<IActionResult> GetStudentSubmissionAndResult(int id)
        {
            if (id <= 0) return BadRequest();
            var submissions = await _submissionManager.GetSubmissionsByStudentId(id);
            if (submissions==null) { return NotFound(); }
            
            return Ok(submissions);
        }

        [Authorize]
        [HttpGet("GetYourResult/{id}")]
        public async Task<IActionResult> GetStudentResults(int id) // truyen submiison id //2
        {
            if(id <= 0) return BadRequest();
            var results = await _resultManager.GetResultsBySubmissionId(id);
            if (results==null) { return NotFound();}
            return Ok(results);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("GetMySubmission")]
        public async Task<IActionResult> GetMySubmission()
        {
            var studentIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(studentIdStr, out var studentId))
            {
                return BadRequest("Invalid user id in token.");
            }
            var submission=await _submissionManager.GetSubmissionsByStudentId(studentId);
            if (submission==null) { return NotFound(); }
            return Ok(submission);
        }
    }
}
