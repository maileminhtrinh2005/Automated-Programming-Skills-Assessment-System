using AssignmentService.Application.DTO;
using AssignmentService.Application.Service;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentService.Controllers
{
    [ApiController]
    [Route("api/Assignment")]
    public class AssignmentController : ControllerBase
    {
        private readonly AssignmentManager _assignment;
        public AssignmentController(AssignmentManager assignment)
        {
            _assignment = assignment;
        }


        [HttpPost("AddAssignment")]
        public async Task<IActionResult> AddAssignment(AssignmentRequest request)
        {
            if (request == null) { return BadRequest(); }
            
            bool check = await _assignment.AddAssignment(request);
            if (!check) {return  BadRequest(); }

            return Ok();
        }

        [HttpGet("GetAssignmentById/{id}")]
        public async Task<IActionResult> GetAssignmentById(int id)
        {
            if (id <= 0) return BadRequest();

            var request = new AssignmentRequest { AssignmentId = id };
            var assignment = await _assignment.GetAssignmentById(request);

            if (assignment == null) return NotFound();

            return Ok(assignment);
        }

        [HttpPost("UpdateAssignment")]
        public async Task<IActionResult> UpdateAssignment(AssignmentRequest request)
        {
            if (request == null) return BadRequest();
            if (! await _assignment.UpdateAssignment(request)) return BadRequest();

            return Ok();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            try
            {

                if (id <= 0) return BadRequest();

                var request = new AssignmentRequest { AssignmentId = id };
                if (!await _assignment.DeleteAssignment(request))
                    return BadRequest();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllAssignment")]
        public async Task<IActionResult> GetAllAssignment()
        {

            var assignmentList = await _assignment.GetAllAssignment();
            if (assignmentList == null) return NotFound();
            return Ok(assignmentList);
        }
    }
}
