namespace FileListener.Cfg
{
    public class AppConfig
    {
        public required RedisConfig RedisConfig { get; init; }
        public required FolderMonitoringConfig FolderMonitoringConfig { get; init; }
        public required MessageQueueConfig MessageQueueConfig { get; init; }
    }

    public class RedisConfig
    {
        public required string FolderCollectionName { get; init; }
    }

    public class FolderMonitoringConfig
    {
        public required int MaxAllowedFolders { get; init; }
        public required int InternalBufferSizeKb { get; init; }

        public required EventInterceptorCfg EventInterceptorCfg { get; init; }
    }

    public class EventInterceptorCfg
    {
        public required int MaxParallelism { get; init; }
        public required int MaxMessagesPerBuffer { get; init; }
    }

    public class MessageQueueConfig
    {
        public required string FileEventSubscriptionId { get; init; }
    }

}
