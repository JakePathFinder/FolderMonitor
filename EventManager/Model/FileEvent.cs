using System.Data;
using Common.DTO;

namespace EventManager.Model
{
    public class FileEvent
    {
        public required string FolderName { get; set; }
        public required string FileName { get; set; }
        public required string EventType { get; set; }
        public DateTime EventDate { get; set; }


        public string GenerateId()
        {
            return $"{FolderName}_{FileName}_{EventType}_{EventDate:o}";
        }

        public static FileEvent From(FileEventEmittedMessage message)
        {
            return new FileEvent()
            {
                EventType = message.EventArgs.ChangeType.ToString(),
                FileName = Path.GetFileName(message.EventArgs.FullPath),
                FolderName = Path.GetDirectoryName(message.EventArgs.FullPath) ?? throw new NoNullAllowedException("Cannot parse FileEvent path to folder name"),
                EventDate = message.HandledDateTimeUtc
            };
        }
    }
}
