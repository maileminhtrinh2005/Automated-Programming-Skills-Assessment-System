using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace NotificationService.Hubs
{
    // 🔹 Interface định nghĩa cho client
    public interface INotificationClient
    {
        Task NotifyNew(NotificationDto dto);
    }

    // 🔹 DTO gửi qua SignalR
    public record NotificationDto(Guid Id, string Title, string Message, DateTime CreatedAtUtc);

    // 🔹 Hub chính
    public class NotificationHub : Hub<INotificationClient>
    {
        // Lưu danh sách kết nối: studentId → danh sách ConnectionId
        private static readonly ConcurrentDictionary<string, List<string>> _connections = new();

        // 🧩 Khi client join group (theo studentId)
        public async Task JoinGroup(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                Console.WriteLine($"⚠️ JoinGroup bị từ chối: studentId rỗng (Conn: {Context.ConnectionId})");
                return;
            }

            // Thêm connectionId vào group
            await Groups.AddToGroupAsync(Context.ConnectionId, studentId);

            _connections.AddOrUpdate(studentId,
                new List<string> { Context.ConnectionId },
                (key, list) =>
                {
                    if (!list.Contains(Context.ConnectionId))
                        list.Add(Context.ConnectionId);
                    return list;
                });

            Console.WriteLine($"✅ [SignalR] Client {Context.ConnectionId} JOINED group {studentId}");
        }

        // 🧩 Khi client rời nhóm (hoặc logout)
        public async Task LeaveGroup(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                Console.WriteLine($"⚠️ LeaveGroup bị từ chối: studentId rỗng (Conn: {Context.ConnectionId})");
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, studentId);

            if (_connections.TryGetValue(studentId, out var list))
            {
                list.Remove(Context.ConnectionId);
                if (list.Count == 0)
                    _connections.TryRemove(studentId, out _);
            }

            Console.WriteLine($"👋 [SignalR] Client {Context.ConnectionId} LEFT group {studentId}");
        }

        // 🧩 Khi client kết nối
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"🔌 [SignalR] Client CONNECTED: {Context.ConnectionId}");

            // Gửi test message cho client để xác nhận kết nối thành công
            await Clients.Caller.NotifyNew(new NotificationDto(
                Guid.NewGuid(),
                "Kết nối thành công ✅",
                "Bạn đã kết nối lại SignalR thành công.",
                DateTime.UtcNow
            ));

            await base.OnConnectedAsync();
        }

        // 🧩 Khi client ngắt kết nối
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var key in _connections.Keys.ToList())
            {
                if (_connections[key].Remove(Context.ConnectionId) && !_connections[key].Any())
                {
                    _connections.TryRemove(key, out _);
                    Console.WriteLine($"❌ [SignalR] Xóa nhóm trống: {key}");
                }
            }

            Console.WriteLine($"❌ [SignalR] Client {Context.ConnectionId} DISCONNECTED");
            await base.OnDisconnectedAsync(exception);
        }

        // 🧩 Hàm tiện ích lấy danh sách connection theo studentId (dùng cho broadcast)
        public static List<string> GetConnections(string studentId)
        {
            return _connections.TryGetValue(studentId, out var list)
                ? list
                : new List<string>();
        }
    }
}
