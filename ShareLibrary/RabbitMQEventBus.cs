﻿using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace ShareLibrary
{
    public class RabbitMQEventBus : IEventBus, IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly JsonSerializerOptions _jsonOptions;

        public RabbitMQEventBus(
            IConnectionFactory connectionFactory,
            IServiceScopeFactory scopeFactory)
        {
            _connectionFactory = connectionFactory;
            _scopeFactory = scopeFactory;
            _handlers = new Dictionary<string, List<Type>>();

            // Configure JSON serialization options for better compatibility
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            try
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();

                // _logger.LogInformation("RabbitMQ connection established");
            }
            catch
            {
                // _logger.LogError(ex, "Could not connect to RabbitMQ");
                throw;
            }
        }

        public void Publish<T>(T @event) where T : class
        {
            var eventName = @event.GetType().Name;
            // _logger.LogInformation("Publishing event {EventName}", eventName);

            try
            {
                _channel.QueueDeclare(queue: eventName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false);

                var message = JsonSerializer.Serialize(@event, _jsonOptions);
                // _logger.LogDebug("Publishing event JSON: {EventJson}", message);
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(exchange: "",
                                     routingKey: eventName,
                                     basicProperties: null,
                                     body: body);

                //_logger.LogInformation("Event {EventName} published successfully", eventName);
            }
            catch (Exception ex)
            {
                //  _logger.LogError(ex, "Error publishing event {EventName}", eventName);
                throw;
            }
        }

        public void Subscribe<T, TH>() where T : class where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());

                _channel.QueueDeclare(queue: eventName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    await ProcessEvent(eventName, message);
                };

                _channel.BasicConsume(queue: eventName,
                                     autoAck: true,
                                     consumer: consumer);
            }

            if (_handlers[eventName].Any(h => h == handlerType))
            {
                //   _logger.LogWarning("Handler Type {HandlerType} already registered for {EventName}", handlerType.Name, eventName);
                return;
            }

            _handlers[eventName].Add(handlerType);
            // _logger.LogInformation("Subscribed to event {EventName} with {HandlerType}", eventName, handlerType.Name);
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (!_handlers.ContainsKey(eventName))
            {
                //   _logger.LogWarning("No handler registered for {EventName}", eventName);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var handlers = _handlers[eventName];

            foreach (var handlerType in handlers)
            {
                try
                {
                    var handler = scope.ServiceProvider.GetService(handlerType);
                    if (handler == null)
                    {
                        //  _logger.LogWarning("Handler {HandlerType} not registered in DI", handlerType.Name);
                        continue;
                    }

                    // Get the event type from the handler's generic argument
                    var eventType = handlerType.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                        ?.GetGenericArguments().FirstOrDefault();

                    if (eventType == null)
                    {
                        //   _logger.LogError("Could not determine event type for handler {HandlerType}", handlerType.Name);
                        continue;
                    }

                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    //  _logger.LogDebug("Deserializing event {EventName} JSON: {EventJson}", eventName, message);
                    var eventData = JsonSerializer.Deserialize(message, eventType, _jsonOptions);

                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new[] { eventData });
                }
                catch (Exception ex)
                {
                    // _logger.LogError(ex, "Error processing event {EventName}", eventName);
                }
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
