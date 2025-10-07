namespace AssignmentService.Application.DTO
{
    public class AssignmentRequest
    {
        // xem thu aii luu bai tap
        public int UserId { get; set; }


        // phan nay nhan request tu client cua bai tap
        //assignment request
        public int AssignmentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public string Difficulty { get; set; }


        // phan nay nhan request cua client phan testcase
        // testcase request
        public int languageId { get; set; }
        public string sourceCode { get; set; }
        public double weight { get; set; }
    }
}
