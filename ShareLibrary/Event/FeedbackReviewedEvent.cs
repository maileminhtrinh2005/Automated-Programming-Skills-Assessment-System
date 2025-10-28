namespace FeedbackService.Application.Events;

public class FeedbackReviewedEvent
{
    public int SubmissionId { get; set; }
    public int StudentId { get; set; } 
    public string InstructorId { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public string FeedbackText { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
}
