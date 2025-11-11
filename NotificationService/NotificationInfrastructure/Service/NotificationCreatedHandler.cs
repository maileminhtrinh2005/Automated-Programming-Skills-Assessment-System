using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Entities;
using NotificationService.Hubs;
using NotificationService.Infrastructure.Persistence;
using ShareLibrary;
using ShareLibrary.Event;

namespace NotificationService.Infrastructure.Handlers
{
    public class NotificationCreatedHandler :
        IEventHandler<NotificationCreatedEvent>,
        IEventHandler<FeedbackGeneratedEvent> 
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

        public async Task Handle(FeedbackGeneratedEvent e)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("[NotificationService] 📩 Received FeedbackGeneratedEvent");
            Console.WriteLine($"StudentId : {e.StudentId}");
            Console.WriteLine($"Title     : {e.Title}");
            Console.WriteLine($"Message   : {e.Message}");
            Console.WriteLine("==========================================");

            var rec = new GeneratedNotificationRecord
            {
                StudentId = e.StudentId,
                Title = e.Title ?? $"Kết quả bài nộp #{e.SubmissionId}",
                Message = string.IsNullOrWhiteSpace(e.Message)
                    ? "Không có nội dung phản hồi."
                    : e.Message,
                CreatedAtUtc = e.CreatedAtUtc,
                 IsRead = false
            };

            await _db.GeneratedNotifications.AddAsync(rec);
            await _db.SaveChangesAsync();

            Console.WriteLine($"✅ [NotificationService] Saved feedback notification Id={rec.Id}");

            if (e.StudentId > 0)
            {
                await _hub.Clients.Group(e.StudentId.ToString())
                    .NotifyNew(new NotificationDto(rec.Id, rec.Title, rec.Message, rec.CreatedAtUtc));

                Console.WriteLine($"📡 [SignalR] Pushed feedback to student {e.StudentId}");
            }
            else
            {
                await _hub.Clients.All
                    .NotifyNew(new NotificationDto(rec.Id, rec.Title, rec.Message, rec.CreatedAtUtc));

                Console.WriteLine("📡 [SignalR] Pushed feedback to all clients (no specific student)");
            }
        }
    }
}
