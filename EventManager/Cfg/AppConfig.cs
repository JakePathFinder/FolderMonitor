namespace EventManager.Cfg
{
    public class AppConfig
    {
        public required MessageQueueConfig MessageQueueConfig { get; init; }
        public required RedisConfig RedisConfig { get; init; }
        public required QueryControllerConfig QueryControllerConfig { get; init; }
    }

    public class MessageQueueConfig
    {
        public required string FileEventSubscriptionId { get; init; }
    }

    public class RedisConfig
    {
        public required string EventDetailsHashCollectionKey { get; init; }
        public required string EventByFolderSortedSetKey { get; init; }
        public required string EventByTypeSortedSetKey { get; init; }
        public required string EventByTimeSortedSetKey { get; init; }
    }

    public class QueryControllerConfig
    {
        public required int MaxAllowedRecordsToReturn { get; init; }
    }

}
