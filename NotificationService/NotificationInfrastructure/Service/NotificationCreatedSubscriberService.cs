
using Microsoft.Extensions.Hosting;
using NotificationService.Infrastructure.Handlers;
using ShareLibrary;
using ShareLibrary.Event;

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
           
            _eventBus.Subscribe<FeedbackGeneratedEvent, NotificationEventHandler>();
            // sub deadline 
            _eventBus.Subscribe<DeadlineNotification, DeadlineNotificationHandler>();



            Console.WriteLine("[NotificationService] ✅ Subscribed to FeedbackGeneratedEvent & FeedbackReviewedEvent");
            return Task.CompletedTask;
        }
    }
}
