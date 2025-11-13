
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _event.Subscribe<TestCaseFetchEvent, RunCodeHandle>();

           

            //return Task.CompletedTask;
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
