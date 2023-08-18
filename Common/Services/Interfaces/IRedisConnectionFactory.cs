using StackExchange.Redis;

namespace Common.Services.Interfaces
{
    public interface IRedisConnectionFactory
    {
        IDatabase GetDatabase();
    }
}
