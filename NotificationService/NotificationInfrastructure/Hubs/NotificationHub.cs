using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs
{
    // (Tùy chọn) Strongly-typed client
    public interface INotificationClient
    {
        Task NotifyNew(NotificationDto dto);
    }

    public record NotificationDto(Guid Id, string Title, string Message, DateTime CreatedAtUtc);

    public class NotificationHub : Hub<INotificationClient>
    {
        // Bạn có thể thêm Group/Authorize nếu cần gửi theo user
    }
}
