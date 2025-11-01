using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NotificationService.Hubs;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Background
{
    /// <summary>
    /// Luồng nền chuyên gửi thông báo chưa gửi qua SignalR
    /// </summary>
    public class NotificationBackgroundWorker : BackgroundService
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub, INotificationClient> _hub;

        public NotificationBackgroundWorker(AppDbContext db, IHubContext<NotificationHub, INotificationClient> hub)
        {
            _db = db;
            _hub = hub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("🧵 [NotificationWorker] Background thread started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 🔎 Lấy các thông báo mới thêm gần đây mà chưa broadcast
                    var pending = await _db.GeneratedNotifications
                        .Where(n => !n.IsBroadcasted)
                        .OrderBy(n => n.CreatedAtUtc)
                        .ToListAsync(stoppingToken);

                    foreach (var noti in pending)
                    {
                        // 📡 Gửi realtime đến tất cả sinh viên
                        await _hub.Clients.All.NotifyNew(new NotificationDto(
                            noti.Id,
                            noti.Title,
                            noti.Message,
                            noti.CreatedAtUtc
                        ));

                        Console.WriteLine($"✅ [NotificationWorker] Broadcast: {noti.Title}");

                        // Đánh dấu đã gửi để không gửi lại
                        noti.IsBroadcasted = true;
                    }

                    if (pending.Any())
                        await _db.SaveChangesAsync(stoppingToken);

                    // ⏳ Chờ 3 giây rồi kiểm tra lại
                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [NotificationWorker] Error: {ex.Message}");
                    await Task.Delay(5000, stoppingToken); // nghỉ 5s nếu lỗi
                }
            }

            Console.WriteLine("🧵 [NotificationWorker] Background thread stopped.");
        }
    }
}
