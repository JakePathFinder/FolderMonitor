namespace Common.DTO
{
    public class FileEventEmittedMessage
    {
        public required FileSystemEventArgs EventArgs { get; init; }
        public required DateTime HandledDateTimeUtc { get; init; }
    }
}
