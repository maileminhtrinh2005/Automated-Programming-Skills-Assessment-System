namespace CourseService.Domain;

public class Course
{
    public Guid Id { get; set; }               // Khóa chính
    public string Code { get; set; } = default!;   // Mã môn học (VD: CS101)
    public string Name { get; set; } = default!;   // Tên môn học
    public string? Description { get; set; }       // Mô tả môn học
    public int Credits { get; set; }               // Số tín chỉ
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Ngày tạo
}
