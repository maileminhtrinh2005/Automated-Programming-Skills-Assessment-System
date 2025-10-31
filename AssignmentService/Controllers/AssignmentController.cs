using AssignmentService.Application.DTO;
using AssignmentService.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [Authorize(Roles = "Admin,Lecturer")]
        [HttpPost("AddAssignment")]
        public async Task<IActionResult> AddAssignment([FromBody]AssignmentRequest request)
        {
            if (request == null) { return BadRequest(); }

            var teacherIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(teacherIdStr, out var teacherId))
            {
                return BadRequest("Invalid user id in token.");
            }
            request.UserId = teacherId;

            bool check = await _assignment.AddAssignment(request);
            if (!check) { return BadRequest(); }

            return Ok();
        }

        [Authorize]
        [HttpGet("GetAssignmentById/{id}")] // id baif tapj assignmentid 
        public async Task<IActionResult> GetAssignmentById(int id)
        {
            if (id <= 0) return BadRequest();

            var request = new AssignmentRequest { AssignmentId = id };
            var assignment = await _assignment.GetAssignmentById(request);
            //Console.WriteLine(assignment.IsHidden);

            if (assignment == null) return NotFound();

            return Ok(assignment);
        }

        [Authorize(Roles = "Lecturer")]
        [HttpPut("UpdateAssignment")]
        public async Task<IActionResult> UpdateAssignment([FromBody]AssignmentRequest request)
        {
            Console.WriteLine("cjealkdjkasldjass");
            if (request == null) return BadRequest("loixixixixixx");
            var teacherIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(teacherIdStr, out var teacherId))
            {
                return BadRequest("Invalid user id in token.");
            }
            request.UserId = teacherId;

            Console.WriteLine("askjhdhajkhjdkasd");
            if (! await _assignment.UpdateAssignment(request)) return BadRequest("adasdasdasdasd");

            return Ok();
        }

        [Authorize(Roles = "Lecturer")]
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


        [Authorize]
        [HttpGet("GetAllAssignment")]
        public async Task<IActionResult> GetAllAssignment()
        {
            Console.WriteLine("alooo");
            var assignmentList = await _assignment.GetAllAssignment();
            if (assignmentList == null) return NotFound();
            return Ok(assignmentList);
        }

        [Authorize]
        [HttpGet("GetAllAssignmentForStudent")]
        public async Task<IActionResult> GetAllAssignmentForStudent()
        {
            Console.WriteLine("cjeaslkdjasd");
            var assignmentList = await _assignment.GetAssignmentForStudent();
            if (assignmentList == null) return NotFound();
            return Ok(assignmentList);
        }

        [Authorize(Roles ="Lecturer")]
        [HttpPut("update-ishidden")]
        public async Task<IActionResult> UpdateIsHidden([FromBody] AssignmentRequest a)
        {
            var isSuccess = await _assignment.UpdateIsHidden(a);
            if (isSuccess == false) return BadRequest();
            return Ok();
        }

    }
}
