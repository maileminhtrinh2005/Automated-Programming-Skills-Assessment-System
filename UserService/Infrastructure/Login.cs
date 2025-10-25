using Microsoft.AspNetCore.Identity;
using UserService.Application.DTO;
using UserService.Application.Interface;
using UserService.Domain;

namespace UserService.Infrastructure
{

    public class Login
    {
        private readonly ICRUD _crud;
        private readonly PasswordHasher<User> _passwordHasher;

        // ✅ sửa constructor để nhận ICRUD (chứ không phải CRUD cụ thể)
        public Login(ICRUD crud)
        {
            _crud = crud;
            _passwordHasher = new PasswordHasher<User>(); // khởi tạo hasher
        }

        public async Task<bool> LoginC(LoginDTO loginDTO)
        {
            var user = await _crud.GetUserByUsername(loginDTO.Username);
            if (user == null)
                return false;

            // ✅ Dùng VerifyHashedPassword để so sánh mật khẩu
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDTO.PasswordHash);

            return result == PasswordVerificationResult.Success;

        }
    }
}
