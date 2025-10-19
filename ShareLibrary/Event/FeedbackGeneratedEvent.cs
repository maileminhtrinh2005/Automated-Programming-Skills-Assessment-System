using System;

namespace ShareLibrary.Event
{
    public class FeedbackGeneratedEvent
    {
        public Guid SubmissionId { get; set; }
        public string? UserEmail { get; set; }
        public double? Score { get; set; }
        public string? ResultStatus { get; set; }
        public string? Feedback { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
