using Common.Services.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace CommonTests.Services
{
    public class RabbitMqServiceTests
    {
        private readonly RabbitMqService _service;
        private readonly ILogger<RabbitMqService> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string SubscriptionId = "testExchange:testQueue";

        public RabbitMqServiceTests()
        {
            _logger = Substitute.For<ILogger<RabbitMqService>>();
            _connection = Substitute.For<IConnection>();
            _channel = Substitute.For<IModel>();
            _connection.CreateModel().Returns(_channel);

            var factory = Substitute.For<IRmqConnectionFactory>();
            factory.CreateConnection().Returns(_connection);

            _service = new RabbitMqService(factory, _logger);
        }

        [Fact]
        public void Send_ShouldSendCorrectMessage()
        {
            var message = new { data = "test" };

            _service.Send(SubscriptionId, message);

            _channel.Received().QueueDeclare(Arg.Any<string>(), false, false, false,
                Arg.Any<IDictionary<string, object>>());
            _channel.Received().BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<ReadOnlyMemory<byte>>());
        }

        [Fact]
        public void Subscribe_ShouldSetupSubscriptionCorrectly()
        {
            _service.Subscribe<object>(SubscriptionId, x => Task.CompletedTask);

            _channel.Received().QueueDeclare(Arg.Any<string>(), false, false, false,
                Arg.Any<IDictionary<string, object>>());
            _channel.Received().BasicConsume(Arg.Any<string>(), true, Arg.Any<EventingBasicConsumer>());
        }

        [Fact]
        public void Unsubscribe_ShouldCloseConnectionAndChannel()
        {
            _service.Unsubscribe(SubscriptionId);

            _channel.Received().Close();
            _connection.Received().Close();
        }

        [Fact]
        public void ParseSubscriptionId_ShouldReturnCorrectNames()
        {
            var (exchangeName, queueName) = RabbitMqService.ParseSubscriptionId(SubscriptionId);

            Assert.Equal("testExchange", exchangeName);
            Assert.Equal("testQueue", queueName);
        }
    }
}
