namespace FeedbackService.Application.Dtos
{
    public class ReviewedFeedbackDto
    {
        public int SubmissionId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public string FeedbackText { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string InstructorId { get; set; } = string.Empty;
    }
}
