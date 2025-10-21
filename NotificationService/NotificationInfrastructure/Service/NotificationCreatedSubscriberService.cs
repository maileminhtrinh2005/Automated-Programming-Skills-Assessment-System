using Microsoft.Extensions.Hosting;
using ShareLibrary;
using ShareLibrary.Event;
using NotificationService.Infrastructure.Handlers;

namespace NotificationService.Infrastructure
{
    public class NotificationCreatedSubscriberService : BackgroundService
    {
        private readonly IEventBus _eventBus;

        public NotificationCreatedSubscriberService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // dang ki + chay background ,,, khi nao co tin se xuw li
            _eventBus.Subscribe<FeedbackGeneratedEvent, NotificationCreatedHandler>();

            Console.WriteLine("[NotificationService] Subscribed to FeedbackGeneratedEvent");

            return Task.CompletedTask;
        }
    }
}
