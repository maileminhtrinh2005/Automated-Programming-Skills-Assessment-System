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
        public async Task JoinGroup(string studentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, studentId);
            Console.WriteLine($"[SignalR] Client {Context.ConnectionId} joined group {studentId}");
        }
        public async Task LeaveGroup(string studentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, studentId);
            Console.WriteLine($"[SignalR] Client {Context.ConnectionId} left group {studentId}");
        }
    }

}
