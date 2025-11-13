using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Entities;
using NotificationService.Hubs;
using NotificationService.Infrastructure.Persistence;
using ShareLibrary;
using ShareLibrary.Event;

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
            Console.WriteLine("📌 [NotificationService] DeadlineNotification received!");

            // ✏️ Lưu DB
            var rec = new GeneratedNotificationRecord
            {
                StudentId = 0, // broadcast
                Title = "📘 Bài tập mới",
                Message = $"{e.Message}\nDeadline: {e.Deadline:dd/MM/yyyy HH:mm}",
                CreatedAtUtc = DateTime.UtcNow,
                IsRead = false
            };

            await _db.GeneratedNotifications.AddAsync(rec);
            await _db.SaveChangesAsync();

            // 🔥 Gửi tới TẤT CẢ người dùng
            await _hub.Clients.All.NotifyNew(
                new NotificationDto(rec.Id, rec.Title, rec.Message, rec.CreatedAtUtc)
            );

            Console.WriteLine("📡 [SignalR] Sent NEW ASSIGNMENT notification to ALL clients!");
        }
    }
}
