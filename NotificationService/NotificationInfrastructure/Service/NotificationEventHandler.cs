using FeedbackService.Application.Events;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Entities;
using NotificationService.Hubs;
using NotificationService.Infrastructure.Persistence;
using ShareLibrary;
using ShareLibrary.Event;

namespace NotificationService.Infrastructure.Handlers
{

    public class NotificationEventHandler :
        IEventHandler<FeedbackGeneratedEvent>,
        IEventHandler<FeedbackReviewedEvent> 
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub, INotificationClient> _hub;

        public NotificationEventHandler(AppDbContext db, IHubContext<NotificationHub, INotificationClient> hub)
        {
            _db = db;
            _hub = hub;
        }

      
        public async Task Handle(FeedbackGeneratedEvent e)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("[NotificationService] 📩 Received FeedbackGeneratedEvent");
            Console.WriteLine($"SubmissionId: {e.SubmissionId}");
            Console.WriteLine($"Feedback: {e.Feedback}");
            Console.WriteLine("==========================================");

            var rec = new GeneratedNotificationRecord
            {
                Title = $"Kết quả bài nộp #{e.SubmissionId}",
                Message = e.Feedback,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _db.GeneratedNotifications.AddAsync(rec);
            await _db.SaveChangesAsync();

            Console.WriteLine($"✅ [NotificationService] Saved auto feedback notification Id={rec.Id}");

            await _hub.Clients.All.NotifyNew(
                new NotificationDto(rec.Id, rec.Title, rec.Message, rec.CreatedAtUtc));

            Console.WriteLine($"📡 [SignalR] Auto feedback pushed to clients");
        }

      
        public async Task Handle(FeedbackReviewedEvent e)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("[NotificationService] 📬 Received FeedbackReviewedEvent");
            Console.WriteLine($"StudentId: {e.StudentId}");
            Console.WriteLine($"InstructorId: {e.InstructorId}");
            Console.WriteLine($"Assignment: {e.AssignmentTitle}");
            Console.WriteLine($"FeedbackText: {e.FeedbackText}");
            Console.WriteLine($"Comment: {e.Comment}");
            Console.WriteLine("==========================================");

            var rec = new GeneratedNotificationRecord
            {
                StudentId = e.StudentId,
                AssignmentTitle = e.AssignmentTitle,
                Title = $"Giảng viên đã gửi nhận xét cho bài {e.AssignmentTitle}",
                Message = $"{e.FeedbackText}\nGhi chú: {e.Comment}",
                CreatedAtUtc = DateTime.UtcNow
            };

            await _db.GeneratedNotifications.AddAsync(rec);
            await _db.SaveChangesAsync();

            Console.WriteLine($"✅ [NotificationService] Saved reviewed feedback notification Id={rec.Id}");

            // 🎯 Gửi riêng đến nhóm SignalR của sinh viên đó
            await _hub.Clients.Group(e.StudentId.ToString())

                   .NotifyNew(new NotificationDto(rec.Id, rec.Title, rec.Message, rec.CreatedAtUtc));

            Console.WriteLine($"📡 [SignalR] Reviewed feedback pushed to student {e.StudentId}");
        }
    }
}
