

using AssignmentService.Application.DTO;
using AssignmentService.Application.Service;
using Microsoft.AspNetCore.Mvc;

namespace assignmentservice.controllers
{
    [ApiController]
    [Route("api/TestCase")]
    public class TestCaseController : Controller
    {
        private readonly TestCaseManager _testcase;
        public TestCaseController(TestCaseManager testcase)
        {
            _testcase = testcase;
        }

        [HttpPost("AddTestCase")]
        public async Task<IActionResult> addtestcase([FromBody]TestCaseRequest request)
        {
            if (request == null) { return BadRequest(); }

           foreach(var tc in request.testCaseItems)
           {
                bool check = await _testcase.AddTestCase(request.AssiginmentId,
                    tc.Input??"",
                    tc.ExpectedOutput??"",
                    tc.Weight
                    );
                if (!check) return BadRequest("Failed to add one or more testcases");
           }
            return Ok("All testcases added successfully");
        }
    }
}
