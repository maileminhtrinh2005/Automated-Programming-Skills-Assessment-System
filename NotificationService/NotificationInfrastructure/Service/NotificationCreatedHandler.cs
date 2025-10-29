using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Entities;
using NotificationService.Hubs;
using NotificationService.Infrastructure.Persistence;
using ShareLibrary;
using ShareLibrary.Event;

namespace NotificationService.Infrastructure.Handlers
{
    /// <summary>
    /// Lắng nghe NotificationCreatedEvent và FeedbackGeneratedEvent từ RabbitMQ.
    /// Khi nhận, lưu vào DB và đẩy thông báo qua SignalR.
    /// </summary>
    public class NotificationCreatedHandler :
        IEventHandler<NotificationCreatedEvent>,
        IEventHandler<FeedbackGeneratedEvent> // ✅ Dùng chung cho cả auto/manual feedback
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub, INotificationClient> _hub;

        public NotificationCreatedHandler(AppDbContext db, IHubContext<NotificationHub, INotificationClient> hub)
        {
            _db = db;
            _hub = hub;
        }

        // 📩 Khi NotificationService tự nhận event tạo thông báo
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

        // 🟢 Khi FeedbackService gửi FeedbackGeneratedEvent (AI hoặc nhập tay)
        public async Task Handle(FeedbackGeneratedEvent e)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("[NotificationService] 📩 Received FeedbackGeneratedEvent");
            Console.WriteLine($"StudentId : {e.StudentId}");
            Console.WriteLine($"Title     : {e.Title}");
            Console.WriteLine($"Message   : {e.Message}");
            Console.WriteLine("==========================================");

            // 1️⃣ Lưu DB
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

            // 2️⃣ Gửi SignalR — nếu có StudentId thì gửi nhóm, ngược lại gửi All
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
