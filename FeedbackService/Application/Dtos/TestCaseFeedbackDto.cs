namespace FeedbackService.Application.Dtos
{
    public class TestCaseFeedbackDto
    {
        // Tên của testcase, ví dụ "Case 1", "Case 2", ...
        public string? Name { get; set; }

        // Nhận xét chi tiết (AI hoặc người chấm sinh ra)
        public string? Comment { get; set; }
    }
}
