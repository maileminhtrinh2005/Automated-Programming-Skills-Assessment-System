using ShareLibrary;
using ShareLibrary.Event;

namespace NotificationService.Infrastructure.Handlers
{
    /// <summary>
    /// Xử lý sự kiện NotificationCreatedEvent
    /// - Được kích hoạt khi có thông báo mới được tạo.
    /// - Có thể dùng để gửi email, push notification, hoặc log lại thông báo.
    /// </summary>
    public class NotificationCreatedHandler : IEventHandler<NotificationCreatedEvent>
    {
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

      
    }
}
