using FeedbackService.Infrastructure.Handlers;
using ShareLibrary;
using ShareLibrary.Event;

namespace FeedbackService.Infrastructure
{
    public class RabbitMqSubscriberService : BackgroundService
    {
        private readonly IEventBus _eventBus;

        public RabbitMqSubscriberService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

       
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventBus.Subscribe<FeedbackGeneratedEvent, GenerateFeedbackHandler>();

            Console.WriteLine("[FeedbackService] Subscribed to CodeSubmittedEvents");

            return Task.CompletedTask;
        }
    }
}
