namespace SubmissionService.Application.DTOs
{
    public class ResultDTO
    {
        // result in get ()
        public int ResultId { get; set; }
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public int TestCaseId { get; set; } 
        public bool Passed {  get; set; }
        public double Score { get; set; }

        // result in logic 
        public string? Status { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
        public double ExecutionTime { get; set; }
        public int MemoryUsed { get; set; }

    }
}
