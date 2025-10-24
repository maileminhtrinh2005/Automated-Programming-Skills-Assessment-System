using ShareLibrary;
using ShareLibrary.Event;

namespace AdminService.Infrastructure
{
    public class ChatBackgroundService : BackgroundService
    {
        private readonly IEventBus _eventBus;

        public ChatBackgroundService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventBus.Subscribe<ChatMessageEvent, ChatMessageHandler>();
            Console.WriteLine("📡 ChatBackgroundService started. Listening for messages...");
            return Task.CompletedTask;
        }
    }
}
