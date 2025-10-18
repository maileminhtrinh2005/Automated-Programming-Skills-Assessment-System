namespace FeedbackService.Application.Dtos;

public class FeedbackResponseDto
{
    public string Summary { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<RubricItemDto>? RubricBreakdown { get; set; }
    public List<TestCaseNoteDto>? TestCaseFeedback { get; set; }
    public List<string>? Suggestions { get; set; }
    public List<string>? NextSteps { get; set; }
}

public class RubricItemDto
{
    public string Criterion { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Max { get; set; }
}

public class TestCaseNoteDto
{
    public string Name { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}
