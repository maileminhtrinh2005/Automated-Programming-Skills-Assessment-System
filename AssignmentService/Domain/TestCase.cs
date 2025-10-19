using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Domain
{
    public class TestCase
    {
        [Key]
        public int TestCaseId { get; set; }
        public int AssignmentId { get; set; }
        public string? Input { get; set; }
        public string? Status { get; set; }
        public string? ExpectedOutput { get; set; }
        public int MemoryUsed { get; set; }
        public double ExecutionTime { get; set; }
        public string? ErrorMessage { get; set; }
        public double Weight { get; set; }

    }
}
