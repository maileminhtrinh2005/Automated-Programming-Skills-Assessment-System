namespace FeedbackService.Application.Dtos
{
    public class FeedbackAutoRequestDto
    {
        public int StudentId { get; set; }
        public int AssignmentId { get; set; }
    }

    public class AssignmentDto
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class SubmissionDto
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string SourceCode { get; set; } = string.Empty;
        public int LanguageId { get; set; }
    }
}
