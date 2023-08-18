using RabbitMQ.Client.Events;

namespace Common.Services
{
    public class RabbitMqService : IMessageQueueService
    {
        private readonly ILogger<RabbitMqService> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const char Separator = ':';

        public RabbitMqService(IRmqConnectionFactory connectionFactory, ILogger<RabbitMqService> logger)
        {
            _logger = logger;
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Send<T>(string subscriptionId, T obj)
        {
            var (exchangeName, queueName) = ParseSubscriptionId(subscriptionId);
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false,
                arguments: null);
            var json = JsonSerializer.Serialize(obj);
            var body = Encoding.UTF8.GetBytes(json);
            _channel.BasicPublish(exchange: exchangeName, routingKey: queueName, basicProperties: null, body: body);
        }

        public void Subscribe<T>(string subscriptionId, Func<T, Task> handler)
        {
            _logger.LogInformation("Subscribing to {subscriptionId}", subscriptionId);
            var (exchangeName, queueName) = ParseSubscriptionId(subscriptionId);
            _channel.ExchangeDeclare(exchange: exchangeName, type: "direct");
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false,
                arguments: null);
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: queueName);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var json = Encoding.UTF8.GetString(body.ToArray());
                var obj = JsonSerializer.Deserialize<T>(json);
                if (obj != null) handler(obj);
            };
            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }


        public void Unsubscribe(string subscriptionId)
        {
            _logger.LogInformation("Unsubscribing from {subscriptionId}", subscriptionId);
            _channel.Close();
            _connection.Close();
        }

        public static (string exchangeName, string queueName) ParseSubscriptionId(string subscriptionId)
        {
            var names = subscriptionId.Split(Separator);
            return (names[0], names[1]);
        }
    }
}
