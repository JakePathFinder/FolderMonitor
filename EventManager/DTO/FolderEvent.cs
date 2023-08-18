namespace EventManager.DTO
{
    public class FolderEvent
    {
        public required string FileName { get; set; }
        public required string EventType { get; set; }
        public DateTime EventDate { get; set; }
    }
}
