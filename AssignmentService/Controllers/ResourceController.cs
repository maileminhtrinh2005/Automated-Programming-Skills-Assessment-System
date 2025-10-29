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
    }
}
