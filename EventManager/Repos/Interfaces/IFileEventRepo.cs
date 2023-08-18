using Common.Repos.Interfaces;
using EventManager.Model;

namespace EventManager.Repos.Interfaces
{
    public interface IFileEventRepo : IRepo<FileEvent>
    {
        Task<List<FileEvent>> QueryLastEventsAsync(int numEvents, string? folderName = null, string? eventType = null);
    }
}
