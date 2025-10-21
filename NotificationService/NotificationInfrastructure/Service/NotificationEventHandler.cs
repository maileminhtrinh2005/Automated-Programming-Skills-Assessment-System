using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using ShareLibrary;                     
using ShareLibrary.Event;               
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence; 

namespace NotificationService.Infrastructure.Handlers
{
    
    public class NotificationEventHandler : IEventHandler<FeedbackGeneratedEvent>
    {
        private readonly AppDbContext _db; // AppDbContext của NotificationService

        public NotificationEventHandler(AppDbContext db)
        {
            _db = db;
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
        }
    }
}
