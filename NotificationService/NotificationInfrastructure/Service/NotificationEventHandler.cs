using ShareLibrary;
using ShareLibrary.Event;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using System.Text.Json;


namespace NotificationService.NotificationInfrastructure.Service
{
    /// <summary>
    /// Handler nhận sự kiện FeedbackGeneratedEvent từ FeedbackService
    /// rồi tạo bản ghi GeneratedNotificationRecord trong database.
    /// </summary>
    public class NotificationEventHandler : IEventHandler<FeedbackGeneratedEvent>
    {
        private readonly INotificationAppService _notificationService;
        private readonly IEventBus _event;


        public NotificationEventHandler(INotificationAppService notificationService, IEventBus eventBus)
        {
            _notificationService = notificationService;
            _event = eventBus;
        }

        public async Task Handle(FeedbackGeneratedEvent e)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("[NotificationService] Received FeedbackGeneratedEvent");
            Console.WriteLine($"User: {e.UserEmail}");
            Console.WriteLine($"Score: {e.Score}");
            Console.WriteLine($"Status: {e.ResultStatus}");
            Console.WriteLine($"Feedback: {e.Feedback}");
            Console.WriteLine("==========================================");

            // 🔹 Tạo thông báo tương thích với cấu trúc GeneratedNotificationRecord
            var notification = new GeneratedNotificationRecord
            {
                StudentId = e.UserEmail ?? "unknown",
                AssignmentTitle = $"Submission {e.SubmissionId}",
                Title = "Feedback Generated",
                Message = $"Your submission has been graded: {e.ResultStatus}. Score: {e.Score}.",
                Level = e.ResultStatus?.ToLower() == "failed" ? "error" : "info",
                RawJson = JsonSerializer.Serialize(e), // lưu toàn bộ dữ liệu event gốc
                CreatedAtUtc = DateTime.UtcNow
            };
           

            // Gọi AppService để lưu vào DB
            await _notificationService.CreateNotification(notification);
            var createdEvent = new NotificationCreatedEvent
            {
                UserEmail = e.UserEmail,
                Title = notification.Title,
                Content = notification.Message,
                CreatedAt = DateTime.UtcNow
            };

            _event.Publish(createdEvent);
        }
    }
}
