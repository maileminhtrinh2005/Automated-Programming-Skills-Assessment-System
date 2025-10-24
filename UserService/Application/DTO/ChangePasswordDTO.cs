namespace UserService.Application.DTO
{
    public class ChangePasswordDTO
    {
        public string Username { get; set; }        // Tên tài khoản
        public string OldPassword { get; set; }     // Mật khẩu cũ
        public string NewPassword { get; set; }     // Mật khẩu mới
    }
}
