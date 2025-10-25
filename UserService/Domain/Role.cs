namespace UserService.Domain
{
    public class Role
    {
        public int RoleID { get; set; }           // Khóa chính
        public string RoleName { get; set; }      // Tên quyền: Admin / Giảng viên / Sinh viên
        public string? Description { get; set; }  // Mô tả chi tiết quyền

        // Navigation property
        public ICollection<User>? Users { get; set; }
    }
}
