namespace NotificationService.Domain.Entities;

public class GeneratedNotificationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string StudentId { get; set; } = default!;
    public string AssignmentTitle { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string Level { get; set; } = "info";
    public string RawJson { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
}
