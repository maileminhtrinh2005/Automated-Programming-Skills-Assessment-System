namespace AssignmentService.Application.DTO
{
    public class ResourceRequest
    {
        public int AssignmentId { get; set; }
        public string ResourceTitle { get; set; } = string.Empty;
        public string ResourceLink { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
    }
}
