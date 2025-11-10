using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedbackService.Domain.Entities
{
   
    public class DetailedFeedback
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubmissionId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string? Summary { get; set; }       // Tóm tắt AI

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
