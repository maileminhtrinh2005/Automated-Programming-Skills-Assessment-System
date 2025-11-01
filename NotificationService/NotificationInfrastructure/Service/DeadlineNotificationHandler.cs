using NotificationService.Hubs;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using ShareLibrary;
using ShareLibrary.Event;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Infrastructure.Handlers
{
    public class DeadlineNotificationHandler : IEventHandler<DeadlineNotification>
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub, INotificationClient> _hub;

        public DeadlineNotificationHandler(AppDbContext db, IHubContext<NotificationHub, INotificationClient> hub)
        {
            _db = db;
            _hub = hub;
        }

        public async Task Handle(DeadlineNotification e)
        {
            Console.WriteLine("📢 [NotificationService] Nhận sự kiện DeadlineNotification:");
            Console.WriteLine($"Nội dung: {e.Message}");
            Console.WriteLine($"Hạn chót: {e.Deadline}");

            // 🧾 Lưu thông báo vào DB
            var noti = new GeneratedNotificationRecord
            {
                Id = Guid.NewGuid(),
                Title = "Bài tập mới",
                Message = $"{e.Message}\n⏰ Deadline: {e.Deadline:dd/MM/yyyy HH:mm}",
                CreatedAtUtc = DateTime.UtcNow,
                IsRead = false,
                 IsBroadcasted = false
            };

            _db.GeneratedNotifications.Add(noti);
            await _db.SaveChangesAsync();

            // 📡 Gửi thông báo realtime tới tất cả sinh viên
            await _hub.Clients.All.NotifyNew(new NotificationDto(
                noti.Id,
                noti.Title,
                noti.Message,
                noti.CreatedAtUtc
            ));

            Console.WriteLine("✅ Đã broadcast thông báo bài tập mới đến tất cả sinh viên!");
        }
    }
}
