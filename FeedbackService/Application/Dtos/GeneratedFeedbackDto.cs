using System;
using System.Collections.Generic;

namespace FeedbackService.Application.Dtos
{
    // DTO trả về cho frontend Feedback
    public class GeneratedFeedbackDto
    {
        public string? AssignmentTitle { get; set; }
        public string? Summary { get; set; }
        public int Score { get; set; }
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
    }



   
}
