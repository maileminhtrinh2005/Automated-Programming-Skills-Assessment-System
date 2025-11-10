namespace FeedbackService.Domain.Entities;

public class GeneratedFeedbackRecord
{
    public int Id { get; set; }
    public int StudentId { get; set; } = default!;
    public string AssignmentTitle { get; set; } = default!;
    public string Summary { get; set; } = default!;
    public int? Score { get; set; }
    public string RawJson { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
