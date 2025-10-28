using Microsoft.AspNetCore.Mvc;
using SubmissionService.Application.Service;

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

        [HttpGet("GetYourSubmission/{id}")] // id hoc sinh //1 
        public async Task<IActionResult> GetYourSubmissionAndResult(int id)
        {
          

            if (id <= 0) return BadRequest();
            var submissions = await _submissionManager.GetSubmissionsByStudentId(id);
            if (submissions==null) { return NotFound(); }
            
            return Ok(submissions);
        }

        [HttpGet("GetYourResult/{id}")]
        public async Task<IActionResult> GetYoutResult(int id) // truyen submiison id //2
        {
            if(id <= 0) return BadRequest();
            var results = await _resultManager.GetResultsBySubmissionId(id);
            if (results==null) { return NotFound();}
            return Ok(results);
        }
    }
}
