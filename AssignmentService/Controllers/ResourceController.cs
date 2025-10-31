using AssignmentService.Application.DTO;
using AssignmentService.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentService.Controllers
{
    [ApiController]
    [Route("api/Resource")]
    public class ResourceController : Controller
    {
        private readonly ResourceManager _resourceManager;
        public ResourceController(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        [Authorize(Roles ="Admin,Lecturer")]
        [HttpPost("AddResource")]
        public async Task<IActionResult> AddResource([FromBody] ResourceRequest request)
        {
            if (request == null) { return BadRequest(); }
            bool isComplete= await _resourceManager.AddResource(request);
            if (!isComplete) { return BadRequest(); }
            return Ok();
        }

        [Authorize]
        [HttpGet("GetResourceById/{id}")]
        public async Task<IActionResult> GetResourceByAssignmentId(int id)
        {
            var resource =await _resourceManager.GetResourceById(id);

            if (resource == null) { return NotFound(); }

            Console.WriteLine("checkkkk");
            return Ok(resource);
        }


        [Authorize(Roles ="Lecturer,Admin")]
        [HttpDelete("delete-resource-by/{id}")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            if (id < 0) { return BadRequest(); }
            bool isSucces = await _resourceManager.DeleteResource(id);
            if (!isSucces) { return BadRequest(); }
            return Ok();
        }

        [Authorize(Roles = "Lecturer,Admin")]
        [HttpPut("update-resource")]
        public async Task<IActionResult> UpdateResource([FromBody] ResourceRequest request)
        {
            if (request==null) { return BadRequest(); }
            bool isSuccess= await _resourceManager.UpdateResource(request);
            if (!isSuccess) { return BadRequest(); }

            return Ok();
        }

    }
}
