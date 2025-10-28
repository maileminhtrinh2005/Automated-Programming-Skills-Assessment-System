using RabbitMQ.Client.Events;
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
        private const string ExchangeName = "apsas_event_bus"; // 🔸 Exchange chung

        public RabbitMQEventBus(IConnectionFactory connectionFactory, IServiceScopeFactory scopeFactory)
        {
            _connectionFactory = connectionFactory;
            _scopeFactory = scopeFactory;
            _handlers = new Dictionary<string, List<Type>>();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            // 🟢 Tạo exchange dùng chung
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);
        }

        public void Publish<T>(T @event) where T : class
        {
            var eventName = @event.GetType().Name;
            var message = JsonSerializer.Serialize(@event, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: ExchangeName,   // 🟢 dùng exchange thay vì ""
                                  routingKey: eventName,
                                  basicProperties: null,
                                  body: body);

            Console.WriteLine($"[RabbitMQ] Published {eventName}");
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

                // 🟢 Liên kết queue với exchange
                _channel.QueueBind(queue: eventName,
                                   exchange: ExchangeName,
                                   routingKey: eventName);

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

            if (_handlers[eventName].All(h => h != handlerType))
                _handlers[eventName].Add(handlerType);

            Console.WriteLine($"[RabbitMQ] Subscribed to {eventName} with {handlerType.Name}");
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (!_handlers.ContainsKey(eventName))
                return;

            using var scope = _scopeFactory.CreateScope();
            var handlers = _handlers[eventName];

            foreach (var handlerType in handlers)
            {
                var handler = scope.ServiceProvider.GetService(handlerType);
                if (handler == null) continue;

                var eventType = handlerType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                    ?.GetGenericArguments().FirstOrDefault();

                var eventData = JsonSerializer.Deserialize(message, eventType, _jsonOptions);
                var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                await (Task)concreteType.GetMethod("Handle")!.Invoke(handler, new[] { eventData });
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
