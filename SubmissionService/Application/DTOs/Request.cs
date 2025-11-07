namespace SubmissionService.Application.DTOs
{
    public class Request
    {
        public int StudentId { get; set; }
        public string SourceCode { get; set; } = string.Empty;
        public int LanguageId { get; set; }//
        public int AssignmentId { get; set; }   
        public int SubmissionId { get; set; }
        public string Stdin {  get; set; } =string.Empty;
        public string LanguageName { get; set; } = string.Empty;//
    }
}
