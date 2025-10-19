namespace FeedbackService.Domain.Entities;

public class SubmissionDetails
{
    public string StudentId { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public string SourceCode { get; set; } = string.Empty;
    public int LanguageId { get; set; }
}
