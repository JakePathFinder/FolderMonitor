using Common.Cfg;
using Microsoft.Extensions.Options;

namespace Common.Services
{
    public class RmqConnectionFactory : IRmqConnectionFactory
    {
        private readonly string _connectionUri;

        public RmqConnectionFactory(IOptions<CommonConfig> cfg)
        {
            _connectionUri = cfg.Value.ConnectionStrings.RabbitMq;
        }
        public IConnection CreateConnection()
        {
            var factory = new ConnectionFactory { Uri = new Uri(_connectionUri) };
            return factory.CreateConnection();
        }
    }
}
