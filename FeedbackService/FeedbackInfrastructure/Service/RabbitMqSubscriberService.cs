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
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_eventBus.Subscribe<TestCaseFetchEvent, GenerateFeedbackHandler>();
            Console.WriteLine("[FeedbackService] Subscribed to TestCaseFetchEvent");
            //return Task.CompletedTask;
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
