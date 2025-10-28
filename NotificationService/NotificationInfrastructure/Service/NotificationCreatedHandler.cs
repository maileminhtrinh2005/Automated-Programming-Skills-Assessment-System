using FeedbackService.Application.Events;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Entities;
using NotificationService.Hubs;
using NotificationService.Infrastructure.Persistence;
using ShareLibrary;
using ShareLibrary.Event;

namespace NotificationService.Infrastructure.Handlers
{
    public class NotificationCreatedHandler :
        IEventHandler<NotificationCreatedEvent>
       
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub, INotificationClient> _hub;

        public NotificationCreatedHandler(AppDbContext db, IHubContext<NotificationHub, INotificationClient> hub)
        {
            _db = db;
            _hub = hub;
        }

        public Task Handle(NotificationCreatedEvent e)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("[NotificationService] Received NotificationCreatedEvent");
            Console.WriteLine($"User: {e.UserEmail}");
            Console.WriteLine($"Title: {e.Title}");
            Console.WriteLine($"Content: {e.Content}");
            Console.WriteLine($"CreatedAt: {e.CreatedAt}");
            Console.WriteLine("==========================================");
            return Task.CompletedTask;
        }

        // 🟢 THÊM PHẦN NÀY: nhận event FeedbackReviewedEvent từ FeedbackService
        public async Task Handle(FeedbackReviewedEvent e)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("[NotificationService] Received FeedbackReviewedEvent");
            Console.WriteLine($"StudentId: {e.StudentId}");
            Console.WriteLine($"FeedbackText: {e.FeedbackText}");
            Console.WriteLine("==========================================");

            // 1️⃣ Lưu vào DB (tận dụng entity cũ)
            var rec = new GeneratedNotificationRecord
            {
                Title = $"Nhận xét mới cho bài nộp #{e.SubmissionId}",
                Message = e.FeedbackText,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _db.GeneratedNotifications.AddAsync(rec);
            await _db.SaveChangesAsync();

            Console.WriteLine($"✅ [NotificationService] Saved feedback notification Id={rec.Id}");

            // 2️⃣ Gửi SignalR cho client theo StudentId
            await _hub.Clients.Group(e.StudentId.ToString())

                 .NotifyNew(new NotificationDto(rec.Id, rec.Title, rec.Message, rec.CreatedAtUtc));

            Console.WriteLine($"📡 [NotificationService] Pushed feedback to student {e.StudentId}");
        }
    }
}
