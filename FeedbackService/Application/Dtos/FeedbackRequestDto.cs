namespace FeedbackService.Application.Dtos;

public class FeedbackRequestDto
{
    public int StudentId { get; set; } 
    public string AssignmentTitle { get; set; } = string.Empty;
    public string? Rubric { get; set; }
    public string SourceCode { get; set; } = string.Empty;
    public int LanguageId { get; set; }
    public List<TestResultDto> TestResults { get; set; } = new(); // BẮT BUỘC
    public int SubmissionId { get; set; }
}

public class TestResultDto
{
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? Input { get; set; }
    public string? Output { get; set; }
    public string? ErrorMessage { get; set; }
    public double ExecutionTime { get; set; }
    public double MemoryUsed { get; set; }
    public double Weight { get; set; }
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
    public string? AssignmentTitle { get; set; }

    // tối thiểu cần danh sách kết quả từng case
    public List<TestcaseItem> TestResults { get; set; } = new();
    public string? SourceCode { get; set; }
    public int? LanguageId { get; set; }
    public string? Rubric { get; set; }
    public double Score { get; set; } 
}