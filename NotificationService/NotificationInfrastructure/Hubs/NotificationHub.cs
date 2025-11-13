using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace NotificationService.Hubs
{
    public interface INotificationClient
    {
        Task NotifyNew(NotificationDto dto);
    }

    public record NotificationDto(Guid Id, string Title, string Message, DateTime CreatedAtUtc);

    public class NotificationHub : Hub<INotificationClient>
    {
        private static readonly ConcurrentDictionary<string, List<string>> _connections = new();

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"🔌 Client CONNECTED: {Context.ConnectionId}");

            // Lấy userId từ token
            var userIdClaim =
                Context.User?.FindFirst("userId") ??
                Context.User?.FindFirst(ClaimTypes.NameIdentifier) ??
                Context.User?.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"));

            if (userIdClaim == null)
            {
                Console.WriteLine("❌ KHÔNG tìm thấy userId trong token! Không thể join group.");
                await base.OnConnectedAsync();
                return;
            }

            string userGroup = userIdClaim.Value;

            // Join vào group theo userId
            await Groups.AddToGroupAsync(Context.ConnectionId, userGroup);

            _connections.AddOrUpdate(userGroup,
                new List<string> { Context.ConnectionId },
                (key, list) =>
                {
                    if (!list.Contains(Context.ConnectionId))
                        list.Add(Context.ConnectionId);
                    return list;
                });

            Console.WriteLine($"🎯 Client {Context.ConnectionId} JOINED GROUP {userGroup} (TOKEN)");

            // Gửi test notify
            await Clients.Caller.NotifyNew(new NotificationDto(
                Guid.NewGuid(),
                "🔌 Kết nối thành công!",
                "SignalR đã join group theo userId trong token.",
                DateTime.UtcNow
            ));

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var group in _connections.Keys.ToList())
            {
                if (_connections[group].Remove(Context.ConnectionId) &&
                    !_connections[group].Any())
                {
                    _connections.TryRemove(group, out _);
                    Console.WriteLine($"🗑 Removed empty group: {group}");
                }
            }

            Console.WriteLine($"❌ Client DISCONNECTED: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
