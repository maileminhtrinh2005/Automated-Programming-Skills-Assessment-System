namespace AssignmentService.Domain
{
    public class Assignment
    {
        public int AssignmentId { get; set; }
        public string Title  { get; set; } =string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SampleTestCases { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Easy";
        public DateTime Deadline { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsHidden { get; set; } 
    }
}
