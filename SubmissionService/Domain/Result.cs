using System.ComponentModel.DataAnnotations;

namespace SubmissionService.Domain
{
    public class Result
    {
        [Key] 
        public int ResultId { get; set; }
        public int SubmissionId { get; set; }
        public int TestCaseId { get; set; }
        public double ExecutionTime { get; set; }
        public bool Passed { get; set; }
        public int MemoryUsed { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
