using Microsoft.Extensions.Hosting;
using ShareLibrary;
using ShareLibrary.Event;
using NotificationService.Infrastructure.Handlers;

namespace NotificationService.Infrastructure
{
    /// <summary>
    /// Dịch vụ chạy nền: lắng nghe sự kiện NotificationCreatedEvent
    /// để gọi NotificationCreatedHandler xử lý.
    /// </summary>
    public class NotificationCreatedSubscriberService : BackgroundService
    {
        private readonly IEventBus _eventBus;

        public NotificationCreatedSubscriberService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventBus.Subscribe<NotificationCreatedEvent, NotificationCreatedHandler>();

            Console.WriteLine("[NotificationService] Subscribed to NotificationCreatedEvent");

            return Task.CompletedTask;
        }
    }
}
