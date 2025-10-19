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

        /// <summary>
        /// Khi service khởi động, đăng ký consumer để lắng nghe CodeSubmittedEvents
        /// </summary>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventBus.Subscribe<CodeSubmittedEvents, GenerateFeedbackHandle>();

            Console.WriteLine("[FeedbackService] Subscribed to CodeSubmittedEvents");

            return Task.CompletedTask;
        }
    }
}
