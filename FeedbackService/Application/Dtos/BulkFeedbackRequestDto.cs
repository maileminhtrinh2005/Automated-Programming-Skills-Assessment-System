using System.Collections.Generic;

namespace FeedbackService.Application.Dtos
{
    public class BulkFeedbackRequestDto
    {
        public int StudentId { get; set; }
        public List<SubmissionForFeedbackDto> Submissions { get; set; } = new();
    }

    public class SubmissionForFeedbackDto
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; }
        public string CreatedAt { get; set; }
        public double? Score { get; set; }
        public string Status { get; set; }
        public string SourceCode { get; set; }
        public List<TestResultDto> TestResults { get; set; } = new();
    }

   
}
