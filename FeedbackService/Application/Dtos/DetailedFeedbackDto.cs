namespace FeedbackService.Application.Dtos
{
    public class DetailedFeedbackDto
    {
        public int StudentId { get; set; }
        public int SubmissionId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string? Summary { get; set; }
  
    }
}
