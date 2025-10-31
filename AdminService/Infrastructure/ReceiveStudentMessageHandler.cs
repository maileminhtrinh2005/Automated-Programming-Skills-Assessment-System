using Microsoft.AspNetCore.SignalR;
using ShareLibrary;
using ShareLibrary.Event;
using AdminService.Infrastructure.Hubs;

namespace AdminService.Infrastructure
{
    public class ChatMessageHandler : IEventHandler<ChatMessageEvent>
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatMessageHandler(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Handle(ChatMessageEvent @event)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", @event.Sender, @event.Message);
            Console.WriteLine($"📩 [ChatHandler] Received from {@event.Sender}: {@event.Message}");
        }
    }

}
