namespace FeedbackService.Application.Dtos;

public class FeedbackResponseDto
{
    public string? Summary { get; set; } = string.Empty;
    public double? Score { get; set; }
    public List<RubricItemDto>? RubricBreakdown { get; set; }
    public List<TestCaseFeedbackDto>? TestCaseFeedback { get; set; }
    public List<string>? Suggestions { get; set; }
    public List<string>? NextSteps { get; set; }
    public string? OverallProgress { get; set; } = string.Empty;

}

public class RubricItemDto
{
    public string Criterion { get; set; } = string.Empty;
    public double? Score { get; set; }
    public double? Max { get; set; }
}




