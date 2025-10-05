namespace AssignmentService.Application.DTO
{
    public class AssignmentDTO
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public string Difficulty { get; set; }
    }
}
