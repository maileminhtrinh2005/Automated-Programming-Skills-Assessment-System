//using AssignmentService.Application.DTO;
//using AssignmentService.Application.Service;
//using Microsoft.AspNetCore.Mvc;

//namespace AssignmentService.Controllers
//{
//    [ApiController]
//    [Route("api/TestCase")]
//    public class TestCaseController : Controller
//    {
//        private readonly TestCaseControl _testcase;
//        public TestCaseController(TestCaseControl testcase)
//        {
//            _testcase = testcase;
//        }

//        [HttpPost("AddTestCase")]
//        public async Task<IActionResult> AddTestCase(RequestTestCase request)
//        {
//            if (request == null) { return BadRequest(); }
//            if (! await _testcase.AddTestCase(request.sourceCode, request.languageId))
//            {
//                return BadRequest();
//            }

//            return Ok();
//        }
//    }
//}
