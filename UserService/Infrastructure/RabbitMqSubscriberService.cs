using ShareLibrary;
using ShareLibrary.Event;
using Microsoft.Extensions.DependencyInjection;

namespace UserService.Infrastructure
{
    public class RabbitMqSubscriberService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventBus _eventBus;

        public RabbitMqSubscriberService(IServiceProvider serviceProvider, IEventBus eventBus)
        {
            _serviceProvider = serviceProvider;
            _eventBus = eventBus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var chatHandler = scope.ServiceProvider.GetRequiredService<ChatMessageHandler>();

                // 📨 Đăng ký lắng nghe sự kiện từ Admin
                _eventBus.Subscribe<ChatMessageEventAdmin, ChatMessageHandler>();

                Console.WriteLine("✅ [UserService] RabbitMQ Subscriber is running... Listening for admin messages.");
            }

            return Task.CompletedTask;
        }
    }
}
