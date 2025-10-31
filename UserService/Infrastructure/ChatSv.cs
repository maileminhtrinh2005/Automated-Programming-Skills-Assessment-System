﻿using ShareLibrary;
using ShareLibrary.Event;
using UserService.Application.Interface;
namespace UserService.Infrastructure;

public class ChatSv : IChat
{
    private readonly IEventBus _eventBus;

    public ChatSv(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public void SendMessageToAdmin(string message)
    {
        var chatEvent = new ChatMessageEvent
        {
            Sender = "user",
            Receiver = "admin",
            Message = message
        };

        _eventBus.Publish(chatEvent);
        Console.WriteLine($"📤 [User → Admin]: {message}");
    }

}
