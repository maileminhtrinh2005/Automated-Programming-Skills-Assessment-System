using AssignmentService.Application;
using AssignmentService.Application.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentService.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AssignmentController : Controller
    {
        private readonly AssignmentControl _assignment;
        public AssignmentController(AssignmentControl assignment)
        {
            _assignment = assignment;
        }


        [HttpPost("AddAssignment")]
        public async Task<IActionResult> AddAssignment(RequestAssignment request)
        {
            if (request == null) { return BadRequest(); }
            
            bool check = await _assignment.AddAssignment(request);
            if (!check) {return  BadRequest(); }

            return Ok();
        }

        [HttpGet("GetAssignmentByid/{id}")]
        public async Task<IActionResult> GetAssignmentById(int id)
        {
            if (id <= 0) return BadRequest();

            var request = new RequestAssignment { AssignmentId = id };
            var assignment = await _assignment.GetAssignmentById(request);

            if (assignment == null) return NotFound();

            return Ok(assignment);
        }

    }
}
