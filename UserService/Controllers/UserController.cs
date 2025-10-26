using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Jwt;
using System;
using System.Security.Claims;
using UserService.Application.DTO;
using UserService.Application.Interface;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class UserController : Controller
    {
        private readonly ICRUD _crud;
        private readonly ILogin _login;
        private readonly IChat _chat;
        private readonly IJwtService _jwtService;
        public UserController(ICRUD crud, ILogin login, IChat chat, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _chat = chat;
            _login = login;
            _crud = crud;
        }

        [Authorize(Roles = "Admin")]
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
            var user = await _login.LoginAsync(loginDto);

            if (user == null)
            {
                return Unauthorized(new { message = "❌ Invalid username or password" });
            }

            if (user.Role == null)
            {
                user.Role = await _crud.GetRoleById(user.RoleID);
            }

            var token = _jwtService.GenerateToken(user.UserID, user.Role?.RoleName ?? "Student");


            return Ok(new
            {
                message = "✅ Login successful",
                username = user.Username,
                roleId = user.RoleID,
                roleName = user.Role?.RoleName,
                token = token

            });
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDTO user)
        {
            if (user == null) return BadRequest("User data is null");

            var success = await _crud.UpdateUser(user);
            if (!success) return NotFound(new { message = "Không tìm thấy user để cập nhật" });

            return Ok(new { message = "✅ Cập nhật thành công!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _crud.DeleteUser(id);
            if (!success) return NotFound(new { message = "Không tìm thấy user để xóa" });

            return Ok(new { message = "🗑️ Xóa thành công!" });
        }
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changeDto)
        {
            if (changeDto == null)
                return BadRequest(new { message = "Thiếu dữ liệu đổi mật khẩu" });

            var result = await _crud.ChangePassword(changeDto);
            if (!result)
                return Unauthorized(new { message = "❌ Sai tài khoản hoặc mật khẩu cũ!" });

            return Ok(new { message = "✅ Đổi mật khẩu thành công!" });
        }


        [Authorize(Roles = "Lecturer")]
        [HttpGet("GetAllStudents")]
        public async Task<IActionResult> GetAllStudents()
        {
            var role = "Student";
            var students = await _crud.GetAllStudents(role);

            if (students == null || !students.Any())
            {
                // Trả về 404 nếu không tìm thấy sinh viên nào
                return NotFound(new { message = "Không tìm thấy sinh viên nào trong hệ thống." });
            }

            // Trả về 200 OK cùng danh sách sinh viên
            return Ok(students);
        }
        [HttpPost("SendMessageToAdmin")]
        public IActionResult SendMessageToAdmin([FromBody] ChatMessageDTO chat)
        {
            if (chat == null || string.IsNullOrWhiteSpace(chat.Message))
                return BadRequest(new { message = "❌ Nội dung tin nhắn trống." });

            _chat.SendMessageToAdmin(chat.Message);
            return Ok(new { message = "✅ Tin nhắn đã được gửi đến admin!" });
        }



    }
}