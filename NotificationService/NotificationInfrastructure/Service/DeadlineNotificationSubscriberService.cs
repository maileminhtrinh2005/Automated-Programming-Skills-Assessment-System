using Microsoft.Extensions.Hosting;
using NotificationService.Infrastructure.Handlers;
using ShareLibrary;
using ShareLibrary.Event;

namespace NotificationService.Background
{
    public class DeadlineNotificationSubscriberService : BackgroundService
    {
        private readonly IEventBus _eventBus;

        public DeadlineNotificationSubscriberService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("🚀 Subscribing to DeadlineNotification...");
            _eventBus.Subscribe<DeadlineNotification, DeadlineNotificationHandler>();

            Console.WriteLine("📡 DeadlineNotification Subscriber started.");
            return Task.CompletedTask;
        }
    }
}
