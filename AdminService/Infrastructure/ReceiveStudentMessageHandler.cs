using Microsoft.AspNetCore.SignalR;
using ShareLibrary;
using ShareLibrary.Event;
using AdminService.Infrastructure.Hubs;

namespace AdminService.Infrastructure
{
    public class ChatMessageHandler : IEventHandler<ChatMessageEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatMessageHandler(IEventBus eventBus, IHubContext<ChatHub> hubContext)
        {
            _eventBus = eventBus;
            _hubContext = hubContext;
        }

        // 📨 Khi nhận được tin nhắn từ student (qua RabbitMQ)
        public async Task Handle(ChatMessageEvent @event)
        {
            if (@event.Receiver == "admin")
            {
                Console.WriteLine($"💬 [Student → Admin]: {@event.Message}");
                // Gửi tin ra cho giao diện admin qua SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Student", @event.Message);
            }
        }

        // 💬 Hàm này dùng để gửi tin nhắn từ admin → student
        public void SendMessageFromAdmin(string message)
        {
            var chatEvent = new ChatMessageEventAdmin
            {
                Sender = "admin",
                Receiver = "student",
                Message = message
            };

            _eventBus.Publish(chatEvent);
            Console.WriteLine($"📤 [Admin → Student]: {message}");
        }
    }
}
