using AdminService.Application.DTO;
using AdminService.Application.Interface;
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

        [HttpPost("AddAPI")]  // 👈 PHẢI có tên này
        public async Task<IActionResult> AddAPI([FromBody] APIConfigDTO api)
        {
            var result = await _service.AddAPI(api);
            return result ? Ok("✅ Thêm API thành công") : BadRequest("❌ Thêm thất bại");
        }
    }
}
