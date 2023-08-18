namespace Common.Services.Interfaces
{
    public interface IRmqConnectionFactory
    {
        IConnection CreateConnection();
    }
}
