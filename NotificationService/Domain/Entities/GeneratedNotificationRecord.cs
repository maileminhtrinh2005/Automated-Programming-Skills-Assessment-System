namespace NotificationService.Domain.Entities;

public class GeneratedNotificationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int StudentId { get; set; }
    public string? AssignmentTitle { get; set; }   // ✅ Cho phép null
    public string? Title { get; set; }             // ✅ Cho phép null
    public string? Message { get; set; }           // ✅ Cho phép null
    public string? Level { get; set; } = "info";
    public string? RawJson { get; set; }           // ✅ Cho phép null
    public DateTime CreatedAtUtc { get; set; }
    public bool IsRead { get; set; } = false;
    public bool IsBroadcasted { get; set; } = false;
}
