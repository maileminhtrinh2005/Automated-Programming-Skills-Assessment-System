namespace AssignmentService.Application.DTO
{
    public class ResourceDTO
    {
        public int ResourceId { get; set; }
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
