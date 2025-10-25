namespace AssignmentService.Application.DTO
{
    public class TestCaseRequest
    {
        public int AssignmentId { get; set; }
        public required List<TestCaseItem> testCaseItems { get; set; }
    }

    public class TestCaseItem
    {
        public string? Input { get; set; }
        public string? ExpectedOutput { get; set; }
        public double Weight { get; set; }
    }
}
