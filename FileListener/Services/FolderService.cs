using Common.DTO;
using Common.Services.Interfaces;
using FileListener.Cfg;
using FileListener.Repos.Interfaces;
using FileListener.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace FileListener.Services
{
    public class FolderService : IFolderService
    {
        private readonly IFolderMonitoringService _monitoringService;
        private readonly IDistributedSetRepo _repo;
        private readonly IUtilitiesService _utilities;
        private readonly int _maxFolders;

        public FolderService(IFolderMonitoringService monitoringService, IDistributedSetRepo repo, IUtilitiesService utilities, IOptions<AppConfig> cfg)
        {
            _repo = repo;
            _utilities = utilities;
            _maxFolders = cfg.Value.FolderMonitoringConfig.MaxAllowedFolders;
            _monitoringService = monitoringService;
        }
        public async Task<bool> AddFolderAsync(string folderName)
        {
            await ValidateAddFolder(folderName);

            var started = await _monitoringService.StartMonitoring(folderName);
            return started;
        }

        public async Task<List<string>> GetAllFoldersAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<bool> RemoveFolderAsync(string folderName)
        {
            await ValidateRemoveFolder(folderName);
            var stopped = await _monitoringService.StopMonitoring(folderName);
            return stopped;
        }

        private async Task ValidateAddFolder(string folderName)
        {
            if (!_utilities.IsValidFolder(folderName))
            {
                throw new ArgumentException($"Inaccessible folder {folderName}");
            }

            var count = await _repo.CountAsync();
            if (count >= _maxFolders)
            {
                throw new Exception($"Max of {_maxFolders} folders are already monitored");
            }

            if (await _repo.ExistsAsync(folderName))
            {
                throw new Exception($"Folder {folderName} already exists");
            }
        }

        private async Task ValidateRemoveFolder(string folderName)
        {
            if (!_utilities.IsValidFolder(folderName))
            {
                throw new ArgumentException($"Inaccessible folder {folderName}");
            }

            var count = await _repo.CountAsync();
            if (count < 1)
            {
                throw new Exception("Cannot remove. No folders are monitored.");
            }
        }
    }
}
