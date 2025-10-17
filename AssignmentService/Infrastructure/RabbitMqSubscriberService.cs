using ShareLibrary;
using ShareLibrary.Event;

namespace AssignmentService.Infrastructure
{
    public class RabbitMqSubscriberService: BackgroundService
    {

        private readonly IEventBus _eventBus;
        public RabbitMqSubscriberService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // dang ki + chay background ,,, khi nao co tin se xuw li         

            _eventBus.Subscribe<CodeSubmittedEvents, TestCaseHandle>();



            return Task.CompletedTask;
        }

    }
}
