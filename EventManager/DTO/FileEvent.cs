namespace EventManager.DTO
{
    public class FileEvent
    {
        public required string FolderName { get; set; }
        public required string FileName { get; set; }
        public required string EventType { get; set; }
        public DateTime EventDate { get; set; }
    }
}
