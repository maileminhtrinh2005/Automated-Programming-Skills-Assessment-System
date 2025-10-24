using ShareLibrary;
using ShareLibrary.Event;

namespace UserService.Infrastructure
{
    // Chỉ xử lý sự kiện nhận được
    public class ChatMessageHandler : IEventHandler<ChatMessageEventAdmin>
    {
        public Task Handle(ChatMessageEventAdmin @event)
        {
            if (@event.Receiver == "user")
            {
                Console.WriteLine($"💬 [Admin → User]: {@event.Message}");
            }
            return Task.CompletedTask;
        }
    }
}
