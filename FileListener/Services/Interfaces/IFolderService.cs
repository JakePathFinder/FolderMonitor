namespace FileListener.Services.Interfaces
{
    public interface IFolderService
    {
        Task<bool> AddFolderAsync(string folderName);
        Task<List<string>> GetAllFoldersAsync();
        Task<bool> RemoveFolderAsync(string folderName);
    }
}
