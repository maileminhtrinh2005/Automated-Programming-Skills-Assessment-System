public class FeedbackGeneratedEvent
{
    public int StudentId { get; set; }
    public int SubmissionId { get; set; }
    public double? Score { get; set; }
    public string Title { get; set; } = "Feedback mới";
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
