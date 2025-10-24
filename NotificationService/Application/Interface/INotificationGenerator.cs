using NotificationService.Application.Dtos;

namespace NotificationService.Application.Interfaces;

public interface INotificationGenerator
{
    Task<NotificationResponseDto> GenerateFromFeedbackAsync(NotificationRequestDto req, CancellationToken ct);
}
