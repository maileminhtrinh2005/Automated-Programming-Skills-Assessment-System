namespace FeedbackService.Application.Dtos
{
    public class FeedbackAutoRequestDto
    {
        public int StudentId { get; set; }
        public int AssignmentId { get; set; }
    }

    public class AssignmentDto
    {
        public string Title { get; set; } = string.Empty;
    }

    public class SubmissionDto
    {
    
        public int AssignmentId { get; set; }
  
        public string SourceCode { get; set; } = string.Empty;
        public int LanguageId { get; set; }
    }
}
