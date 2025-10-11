namespace FeedbackService.Application.Dtos;

public class ManualFeedbackRequestDto
{
    public string StudentId { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class ManualFeedbackResponseDto
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
