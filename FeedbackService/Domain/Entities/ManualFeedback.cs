namespace FeedbackService.Domain.Entities;

public class ManualFeedback
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty; 
    public int? Score { get; set; }                           
    public string Content { get; set; } = string.Empty;    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
