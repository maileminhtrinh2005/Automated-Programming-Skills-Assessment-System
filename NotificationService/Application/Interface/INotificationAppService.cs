using NotificationService.Application.Dtos;

public interface INotificationAppService
{
    Task<NotificationResponseDto> CreateFromFeedbackAsync(NotificationRequestDto req, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationResponseDto>> GetAllAsync(int take = 50, CancellationToken ct = default);
}
