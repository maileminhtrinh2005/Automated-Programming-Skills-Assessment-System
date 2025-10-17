namespace AssignmentService.Domain
{
    public class Assignment
    {
        public int AssignmentId { get; set; }
        public string? Title  { get; set; } 
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public DateTime Deadline { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
