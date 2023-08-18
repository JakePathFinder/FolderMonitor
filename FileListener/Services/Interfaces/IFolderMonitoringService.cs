namespace FileListener.Services.Interfaces
{
    public interface IFolderMonitoringService : IHostedService
    {
        bool StartMonitoring(string folderName);
        bool StopMonitoring(string folderName);
    }
}
