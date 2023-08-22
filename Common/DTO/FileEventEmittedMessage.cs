namespace Common.DTO
{
    public class FileEventEmittedMessage
    {
        public required string FullPath { get; set; }
        public required string ChangeType { get; set; }
        public DateTime EventDate { get; set; }
    }
}
