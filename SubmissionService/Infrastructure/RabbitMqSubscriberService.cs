
using ShareLibrary;
using ShareLibrary.Event;

namespace SubmissionService.Infrastructure
{
    public class RabbitMqSubscriberService : BackgroundService
    {

        private readonly IEventBus _event;

        public RabbitMqSubscriberService(IEventBus eventBus)
        {
            _event = eventBus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _event.Subscribe<CodeSubmittedEvents, RunCodeHandle>();



            return Task.CompletedTask;
        }
    }
}
