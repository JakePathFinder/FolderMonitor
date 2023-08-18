using Common.Cfg;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Common.Services
{
    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        private readonly IConnectionMultiplexer _connection;

        public RedisConnectionFactory(IOptions<CommonConfig> cfg)
        {
            var connectionString = cfg.Value.ConnectionStrings.Redis;
            _connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public IDatabase GetDatabase()
        {
            return _connection.GetDatabase();
        }
    }
}
