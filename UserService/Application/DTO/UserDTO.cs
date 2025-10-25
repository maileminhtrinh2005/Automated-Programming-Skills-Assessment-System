namespace UserService.Application.DTO
{
    public class UserDTO
    {
        public int UserID { get; set; } // Khóa chính
        public string Username { get; set; } // Tài khoản (unique)
        public string Email { get; set; } // Email (unique)                                         
        public string PasswordHash { get; set; } // Mật khẩu (hash)                                         

        public string FullName { get; set; } // Họ tên                                          


    }
}
