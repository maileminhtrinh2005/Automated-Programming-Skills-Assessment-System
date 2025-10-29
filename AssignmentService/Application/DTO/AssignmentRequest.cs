namespace AssignmentService.Application.DTO
{
    public class AssignmentRequest
    {
        // xem thu aii luu bai tap
        public int UserId { get; set; }// teacher
         
        // phan nay nhan request tu client cua bai tap
        //assignment request
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; }=string.Empty;
        public string SampleTestCase { get; set; } = string.Empty;
        public DateTime Deadline { get; set; } = DateTime.Now;
        public string Difficulty { get; set; } = string.Empty;

    }
}
