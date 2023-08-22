namespace FileListener.Services.Interfaces
{
    public interface IFolderMonitoringService : IHostedService
    {
        Task<bool> StartMonitoring(string folderName);
        Task<bool> StopMonitoring(string folderName);
    }
}
