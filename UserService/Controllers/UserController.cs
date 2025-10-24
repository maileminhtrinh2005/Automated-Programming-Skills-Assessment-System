using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public UserController(ICRUD crud, ILogin login, IChat chat) 
        {
            _chat = chat;
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

            // ▼ SỬA Ở ĐÂY ▼
            // 1. Gọi và *nhận* kết quả từ service
            var user = await _login.LoginAsync(loginDto);

            // 2. Kiểm tra kết quả
            if (user == null)
            {
                // Nếu user là null, nghĩa là login thất bại (sai tên hoặc sai mật khẩu)
                return Unauthorized(new { message = "❌ Invalid username or password" });
            }

            // 3. Trả về thành công nếu user không phải là null
            return Ok(new
            {
                message = "✅ Login successful",
                username = user.Username,
                roleId = user.RoleID,
                roleName = user.Role?.RoleName
                // Lưu ý: user trả về từ Login.cs có thể chưa Include(Role)
                // Bạn có thể cần sửa Login.cs để nó Include Role
                // hoặc lấy RoleName từ _crud.GetUserByUsername sau khi đã xác thực
            });
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
        [HttpGet("GetAllStudents")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _crud.GetAllStudents();

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