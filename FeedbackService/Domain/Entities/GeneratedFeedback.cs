namespace FeedbackService.Domain.Entities;

public class GeneratedFeedback
{
    public string Summary { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<RubricItem> RubricBreakdown { get; set; } = new();
    public List<TestCaseNote> TestCaseFeedback { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public List<string> NextSteps { get; set; } = new();
}
public class RubricItem { public string Criterion { get; set; } = ""; public int Score { get; set; } public int Max { get; set; } }
public class TestCaseNote { public string Name { get; set; } = ""; public string Comment { get; set; } = ""; }
