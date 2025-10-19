namespace FeedbackService.Application.Dtos;

public class FeedbackRequestDto
{
    public string StudentId { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public string? Rubric { get; set; }
    public string SourceCode { get; set; } = string.Empty;
    public int LanguageId { get; set; }
    public List<TestResultDto> TestResults { get; set; } = new(); // BẮT BUỘC
}

public class TestResultDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Passed / Failed
    public string? Input { get; set; }
    public string? Expected { get; set; }
    public string? Actual { get; set; }
    public string? Stdout { get; set; }
    public string? Stderr { get; set; }
    public int? ElapsedMs { get; set; }
}
