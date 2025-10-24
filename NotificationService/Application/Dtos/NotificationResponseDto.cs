namespace NotificationService.Application.Dtos;

public class NotificationResponseDto
{
    public string Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string Level { get; set; } = "info"; // info|success|warning|error
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
