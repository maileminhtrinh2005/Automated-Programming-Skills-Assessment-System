namespace SubmissionService.Application.DTOs
{
    public class Request
    {
        public string? SourceCode { get; set; }
        public int LanguageId { get; set; }
        public int AssignmentId { get; set; }   

        public int SubmissionId { get; set; }
    }
}
