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
// cham test case
public class TestcaseItem
{
    public string? Name { get; set; }          // gán "Case 1", "Case 2"... nếu không có
    public string Status { get; set; } = "Unknown";  // Passed/Failed/Timeout/CE
    public string? Input { get; set; }
    public string? ExpectedOutput { get; set; }
    public string? Output { get; set; }        // actual
    public double? ExecutionTime { get; set; }
    public int? MemoryUsed { get; set; }
    public string? ErrorMessage { get; set; }
    public double? Weight { get; set; }
}
public class TestcaseFeedbackRequestDto
{
    public int StudentId { get; set; }
    public int AssignmentId { get; set; }
    public int SubmissionId { get; set; }

    // tối thiểu cần danh sách kết quả từng case
    public List<TestcaseItem> TestResults { get; set; } = new();
    public string? SourceCode { get; set; }
    public int? LanguageId { get; set; }
    public string? Rubric { get; set; }
    public double Score { get; set; } 
}