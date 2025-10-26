namespace UserService.Application.DTO
{
    public class LoginDTO
    {
        public string Username { get; set; } // Tài khoản (unique)                                    
        public string Password { get; set; } // Mật khẩu (hash)



    }
}
