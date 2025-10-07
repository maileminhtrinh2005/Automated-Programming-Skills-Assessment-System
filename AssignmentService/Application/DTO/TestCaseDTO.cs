using System.Runtime.CompilerServices;

namespace AssignmentService.Application.DTO
{
    public class TestCaseDTO
    {
        public int TestCaseId { get; set; }
        public int AssignmentId { get; set; }
        public string Input { get; set; }
        public string Status { get; set; }
        public string ExpectedOutput { get; set; }
        public int MemoryUsed { get; set; }
        public double ExecutionTime { get; set; }
        public string ErrorMessage { get; set; }
        public double Weight { get; set; }
    }
}
