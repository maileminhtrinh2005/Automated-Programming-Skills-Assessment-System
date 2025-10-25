namespace FeedbackService.Domain.Entities
{
    public class GenerateFeedbackDetail
    {
        public int Id { get; set; }
        public int GenerateFeedbackId { get; set; }     
        public string CaseName { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string? Input { get; set; }
        public string? Expected { get; set; }
        public string? Actual { get; set; }
        public string? Comment { get; set; }

        public GeneratedFeedbackRecord? GenerateFeedback { get; set; }
    }
}
