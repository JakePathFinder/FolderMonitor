namespace Common.Services.Interfaces
{
    public interface IMessageQueueService
    {
        void Send<T>(string subscriptionId, T obj);
        void Subscribe<T>(string subscriptionId, Func<T, Task> handler);
        void Unsubscribe(string subscriptionId);
    }
}
