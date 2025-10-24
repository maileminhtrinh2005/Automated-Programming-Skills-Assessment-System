namespace UserService.Domain
{
    public class User
    {
        public int UserID { get; set; } // Khóa chính
        public string Username { get; set; } // Tài khoản (unique)
        public string Email { get; set; } // Email (unique)                                         
        public string PasswordHash { get; set; } // Mật khẩu (hash)                                         
        public string FullName { get; set; } // Họ tên                                          

        public int RoleID { get; set; } // FK tới Role                                        

        public DateTime CreatedAt { get; set; } // Ngày tạo                                     
        public DateTime UpdatedAt { get; set; } // Ngày cập nhật
                                                
        }
                                                
    }
