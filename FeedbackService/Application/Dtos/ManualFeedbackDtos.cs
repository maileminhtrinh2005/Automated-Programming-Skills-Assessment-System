namespace FeedbackService.Application.Dtos;

public class ManualFeedbackRequestDto
{
    public int StudentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class ManualFeedbackResponseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
public class ManualFeedbackDto
{
    public int SubmissionId { get; set; }
    public int StudentId { get; set; }
    public string InstructorId { get; set; } = string.Empty; // ID giảng viên gửi nhận xét
    public string AssignmentTitle { get; set; } = string.Empty; // Tên bài tập / bài thi
    public string FeedbackText { get; set; } = string.Empty;    // Phần nhận xét chính
    public string Comment { get; set; } = string.Empty;         // Ghi chú / bổ sung
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow; // Thời gian gửi
}
