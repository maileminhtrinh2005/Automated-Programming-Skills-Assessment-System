namespace AssignmentService.Application.DTO
{
    public class AssignmentDTO
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SampleTestCase { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public string Difficulty { get; set; } = string.Empty;


        //resource gui kem khi tao assigment
        public string ResourceTitle { get; set; } = string.Empty;
        public string ResourceLink { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
    }
}
