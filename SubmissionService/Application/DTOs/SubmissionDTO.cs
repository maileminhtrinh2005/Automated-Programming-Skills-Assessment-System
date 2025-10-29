namespace SubmissionService.Application.DTOs
{
    public class SubmissionDTO
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string Code { get; set; } =  string.Empty;
        public int LanguageId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public double Score { get; set; }
        public string LanguageName { get; set; }=string.Empty;
    }
}
