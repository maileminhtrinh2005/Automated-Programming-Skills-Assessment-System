using System.Collections.Generic;

namespace FeedbackService.Application.Dtos
{
    public class BulkFeedbackRequestDto
    {
        public List<SubmissionForFeedbackDto> Submissions { get; set; } = new();
    }

    public class SubmissionForFeedbackDto
    {
        public int SubmissionId { get; set; }
        public string AssignmentTitle { get; set; }
        public double? Score { get; set; }
        public string Status { get; set; }
    }

   
}
