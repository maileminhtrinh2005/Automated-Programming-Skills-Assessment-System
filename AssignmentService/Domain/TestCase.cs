using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Domain
{
    public class TestCase
    {
        [Key]
        public int TestCaseId { get; set; }
        public int AssignmentId { get; set; }
        public string Input { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ExpectedOutput { get; set; } = string.Empty;
        public int MemoryUsed { get; set; }
        public double ExecutionTime { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public double Weight { get; set; }

    }
}
