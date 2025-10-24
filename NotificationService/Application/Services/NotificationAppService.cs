using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using System.Text.Json;

namespace NotificationService.Application.Services;

public class NotificationAppService : INotificationAppService
{
    private readonly INotificationGenerator _generator;
    private readonly AppDbContext _db;

    public NotificationAppService(INotificationGenerator generator, AppDbContext db)
    {
        _generator = generator;
        _db = db;
    }

    public async Task<NotificationResponseDto> CreateFromFeedbackAsync(
        NotificationRequestDto req, CancellationToken ct = default)
    {
        // 1️⃣ Gọi generator để tạo thông báo từ Feedback
        var result = await _generator.GenerateFromFeedbackAsync(req, ct);

        // 2️⃣ Lưu record lại DB (giống FeedbackAppService)
        var record = new GeneratedNotificationRecord
        {
            StudentId = req.StudentId,
            AssignmentTitle = req.AssignmentTitle,
            Title = result.Title,
            Message = result.Message,
            Level = result.Level,
            RawJson = JsonSerializer.Serialize(result),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.GeneratedNotifications.Add(record);
        await _db.SaveChangesAsync(ct);

        // 3️⃣ Trả về DTO cho client
        return result;
    }

    public async Task CreateNotification(GeneratedNotificationRecord notification)
    {
        _db.GeneratedNotifications.Add(notification);
        await _db.SaveChangesAsync();
        Console.WriteLine($"[NotificationService] Saved notification for {notification.StudentId}");
    }

    public async Task<IReadOnlyList<NotificationResponseDto>> GetAllAsync(
        int take = 50, CancellationToken ct = default)
    {
        var list = _db.GeneratedNotifications
                      .OrderByDescending(n => n.CreatedAtUtc)
                      .Take(take)
                      .Select(n => new NotificationResponseDto
                      {
                          Id = n.Id.ToString("N"),
                          Title = n.Title,
                          Message = n.Message,
                          Level = n.Level,
                          CreatedAt = n.CreatedAtUtc
                      }).ToList();

        return await Task.FromResult(list);
    }
}
