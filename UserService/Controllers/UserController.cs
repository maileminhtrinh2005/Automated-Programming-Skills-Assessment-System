using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTO; 
using UserService.Application.Interface; 
using UserService.Infrastructure; 
namespace UserService.Controllers 
{ 
    [ApiController]
    [Route("api/[Controller]")] 
    public class UserController : Controller 
    { 
        private readonly Login _login; 
        private readonly ICRUD _crud; 
        public UserController(ICRUD crud, Login login) 
        {
            _login = login;
            _crud = crud; 
        } 
        [HttpPost("AddUser")] 
        public async Task<IActionResult> AddUser(UserDTO user) 
        { 
            if (user != null) 
            {
                await _crud.AddUser(user); 
            } 
            return Ok(); 
        }
        [AllowAnonymous]
        [HttpPost("Login")] 
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto) 
        { 
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);  
            bool isValid = await _login.LoginC(loginDto); 
            if (isValid) { // 👉 Có thể thêm JWT ở đây (mình sẽ giúp nếu bạn muốn)
                return Ok(new { message = "✅ Login successful", username = loginDto.Username });
            } 
            return Unauthorized(new { message = "❌ Invalid username or password" }); 
        }
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _crud.GetAllUsers();

            if (users == null || !users.Any())
            {
                return NotFound(new { message = "Không tìm thấy người dùng nào trong hệ thống." });
            }

            return Ok(users);
        }
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDTO user)
        {
            if (user == null) return BadRequest("User data is null");

            var success = await _crud.UpdateUser(user);
            if (!success) return NotFound(new { message = "Không tìm thấy user để cập nhật" });

            return Ok(new { message = "✅ Cập nhật thành công!" });
        }

        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _crud.DeleteUser(id);
            if (!success) return NotFound(new { message = "Không tìm thấy user để xóa" });

            return Ok(new { message = "🗑️ Xóa thành công!" });
        }


    }
}