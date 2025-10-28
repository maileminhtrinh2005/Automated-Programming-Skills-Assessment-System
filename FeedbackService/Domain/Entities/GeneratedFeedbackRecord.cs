namespace FeedbackService.Domain.Entities;

public class GeneratedFeedbackRecord
{
    public int Id { get; set; }

    // context bài nộp
    public int StudentId { get; set; } = default!;
    public string AssignmentTitle { get; set; } = default!;

    // dữ liệu cơ bản để list nhanh
    public string Summary { get; set; } = default!;
    public int? Score { get; set; }

    // toàn bộ JSON phản hồi AI để truy vết/hiển thị
    public string RawJson { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
