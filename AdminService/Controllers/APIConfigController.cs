using AdminService.Application.DTO;
using AdminService.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/APIConfig")]
    public class APIConfigController : ControllerBase
    {
        private readonly IAPIConfigService _service;
        public APIConfigController(IAPIConfigService service)
        {
            _service = service;
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("AddAPI")]  // 👈 PHẢI có tên này
        public async Task<IActionResult> AddAPI([FromBody] APIConfigDTO api)
        {
            var result = await _service.AddAPI(api);
            return result ? Ok("✅ Thêm API thành công") : BadRequest("❌ Thêm thất bại");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateAPI/{id}")]
        public async Task<IActionResult> UpdateAPI(int id, [FromBody] APIConfigDTO api)
        {
            var result = await _service.UpdateAPI(id, api);
            return result ? Ok("✅ Cập nhật thành công") : NotFound("❌ Không tìm thấy API cần cập nhật");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllAPI")]
        public async Task<IActionResult> GetAllAPI()
        {
            var result = await _service.GetAllAPI();
            return Ok(result);
        }



    }
}
