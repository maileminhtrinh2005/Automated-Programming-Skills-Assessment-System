namespace SubmissionService.Domain
{
    public class Submission
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string Code { get; set; }
        public int LanguageId { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public string Status { get; set; }
        public double Score { get; set; }
    }
}
