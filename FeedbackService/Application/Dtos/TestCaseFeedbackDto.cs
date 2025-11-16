namespace FeedbackService.Application.Dtos
{
    public class TestCaseFeedbackDto
    {

        public string? Status { get; set; }
        public string? Input { get; set; }
        public string? ExpectedOutput { get; set; }
    
        public string? Name { get; set; }
        public string? Comment { get; set; }
    }
}
