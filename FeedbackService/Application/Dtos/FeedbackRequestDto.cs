namespace FeedbackService.Application.Dtos;

public class FeedbackRequestDto
{
    public int StudentId { get; set; } 
    public string AssignmentTitle { get; set; } = string.Empty;
    public string? Rubric { get; set; }
    public string SourceCode { get; set; } = string.Empty;
    public int LanguageId { get; set; }
    public List<TestResultDto> TestResults { get; set; } = new(); 
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
    public string Status { get; set; } = "Unknown"; 
    public string? Input { get; set; }
    public string? ExpectedOutput { get; set; }
  

}
public class TestcaseFeedbackRequestDto
{
    public int StudentId { get; set; }
    public int AssignmentId { get; set; }
    public int SubmissionId { get; set; }
    public string? AssignmentTitle { get; set; }
    public List<TestcaseItem> TestResults { get; set; } = new();
    public string? SourceCode { get; set; }


   
}