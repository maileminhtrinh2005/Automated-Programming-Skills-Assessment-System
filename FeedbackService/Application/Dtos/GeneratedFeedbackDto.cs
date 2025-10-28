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

        // Chi tiết từng test case
        public List<TestCaseItemDto> Details { get; set; } = new();

        // Thông tin định danh tối thiểu (tránh lệ thuộc SubmissionDto)
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
    }

    // Item chi tiết test case
    public class TestCaseItemDto
    {
        public int TestCaseId { get; set; }
        public double? Weight { get; set; }
        public string? Status { get; set; }           // "Passed"/"Failed"
        public string? Input { get; set; }
        public string? Expected { get; set; }
        public double? ExecutionTimeMs { get; set; }
        public int? MemoryUsed { get; set; }
        public string? Actual { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // (Tuỳ bạn dùng) DTO request khi gọi /feedbacksubmit từ controller/service
   
}
