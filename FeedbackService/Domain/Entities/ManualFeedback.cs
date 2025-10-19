namespace FeedbackService.Domain.Entities;

public class ManualFeedback
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty; // hoặc tên
    public int? Score { get; set; }                           // tuỳ chọn
    public string Content { get; set; } = string.Empty;       // nội dung GV nhập
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
