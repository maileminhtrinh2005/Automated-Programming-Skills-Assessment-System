
using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Entities;
using NotificationService.Hubs;
using NotificationService.Infrastructure.Persistence;
using ShareLibrary;
using ShareLibrary.Event;

namespace NotificationService.Infrastructure.Handlers
{

    public class NotificationEventHandler :
        IEventHandler<FeedbackGeneratedEvent>
  
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
            Console.WriteLine("[NotificationService] 📩 Received FeedbackGeneratedEvent");

            var message = string.IsNullOrWhiteSpace(e.Message)
                                ? "Không có nội dung phản hồi."
                                : e.Message;
            var rec = new GeneratedNotificationRecord
            {
                Title = $"Kết quả bài nộp #{e.SubmissionId}",
                Message = message,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _db.GeneratedNotifications.AddAsync(rec);
            await _db.SaveChangesAsync();

            await _hub.Clients.All.NotifyNew(new NotificationDto(rec.Id, rec.Title, rec.Message, rec.CreatedAtUtc));
            Console.WriteLine($"📡 [SignalR] Auto feedback pushed to clients");
        }


      
    }
}
