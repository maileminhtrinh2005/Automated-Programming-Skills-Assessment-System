using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Hubs;
using NotificationService.Infrastructure.Persistence; 
using ShareLibrary;                     
using ShareLibrary.Event;               
using System;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.Handlers
{
    
    public class NotificationEventHandler : IEventHandler<FeedbackGeneratedEvent>
    {
        private readonly AppDbContext _db; // AppDbContext của NotificationService
        private readonly IHubContext<NotificationHub, INotificationClient> _hub;

        public NotificationEventHandler(AppDbContext db, IHubContext<NotificationHub, INotificationClient> hub)
        {
            _db = db;
            _hub = hub;
        }

        public async Task Handle(FeedbackGeneratedEvent e)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("[NotificationService] Received FeedbackGeneratedEvent");
            
            Console.WriteLine($"Feedback  : {e.Feedback}");
       
            Console.WriteLine("=> Saving to database...");
            Console.WriteLine("==========================================");

            var rec = new GeneratedNotificationRecord
            {
              
               
                Title = $"Kết quả bài nộp #{e.SubmissionId.ToString()[..8]}",
                Message = e.Feedback,
                CreatedAtUtc = DateTime.UtcNow,
               


            };

            await _db.GeneratedNotifications.AddAsync(rec);
            await _db.SaveChangesAsync();

            Console.WriteLine($"[NotificationService] ✅ Saved notification Id={rec.Id}");
            await _hub.Clients.All.NotifyNew(
                new NotificationDto(rec.Id, rec.Title, rec.Message, rec.CreatedAtUtc));

            Console.WriteLine("[NotificationService] 📡 SignalR pushed to clients");
        }
    }
}
