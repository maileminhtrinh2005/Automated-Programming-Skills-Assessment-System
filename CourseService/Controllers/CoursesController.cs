using CourseService.Application; // ICourseService, DTOs
using CourseService.Application.Dtos;
using CourseService.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CourseService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : Controller
    {
        private readonly ICourseService _service;

        public CoursesController(ICourseService service)
        {
            _service = service;
        }

        // GET /api/courses
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllAsync();
                return Ok(new { message = "OK", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server (GetAll)", detail = ex.Message });
            }
        }

        // GET /api/courses/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await _service.GetByIdAsync(id);
                if (data == null) return NotFound(new { message = "Không tìm thấy" });
                return Ok(new { message = "OK", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server (GetById)", detail = ex.Message });
            }
        }

        // POST /api/courses
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest(new { message = "Code và Name là bắt buộc" });

                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { message = "Tạo thành công", data = created });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server (Create)", detail = ex.Message });
            }
        }

        // PUT /api/courses/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CourseUpdateDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                if (updated == null) return NotFound(new { message = "Không tìm thấy để cập nhật" });
                return Ok(new { message = "Cập nhật thành công", data = updated });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server (Update)", detail = ex.Message });
            }
        }

        // DELETE /api/courses/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var ok = await _service.DeleteAsync(id);
                if (!ok) return NotFound(new { message = "Không tìm thấy để xoá" });
                return Ok(new { message = "Xoá thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server (Delete)", detail = ex.Message });
            }
        }
    }
}
