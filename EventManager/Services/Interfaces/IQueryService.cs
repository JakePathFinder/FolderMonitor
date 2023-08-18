using EventManager.Model;

namespace EventManager.Services.Interfaces
{
    public interface IQueryService
    {
        Task<List<FileEvent>> QueryAllByDateAsync(int numEvents);
        Task<List<FileEvent>> QueryByFolderAsync(int numEvents, string? folderName = null);
        Task<List<FileEvent>> QueryByEventAsync(int numEvents, string? eventType = null);
    }
}
