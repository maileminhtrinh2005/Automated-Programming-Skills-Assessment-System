namespace FeedbackService.Domain.Entities;

public class SubmissionDetails
{
    public int StudentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public string SourceCode { get; set; } = string.Empty;
    public int LanguageId { get; set; }
}
