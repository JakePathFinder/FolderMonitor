using Common.Services.Interfaces;
using FileListener.Cfg;
using FileListener.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Threading.Tasks.Dataflow;
using Common.DTO;
using FileListener.Constants;
using FileListener.Repos.Interfaces;
using FileListener.Model;

namespace FileListener.Services
{
    public class FolderMonitoringService : IFolderMonitoringService
    {
        private readonly IMessageQueueService _msgQService;
        private readonly ILogger<FolderMonitoringService> _logger;
        private readonly IDistributedSetRepo _repo;
        private readonly Dictionary<string, FileSystemWatcherWrapper> _watchers = new();
        private readonly int _watcherBufferSize;
        private readonly string _subscriptionId;
        private readonly ActionBlock<FileSystemEventArgs> _eventInterceptor;
        private readonly ErrorEventHandler _errorHandler;

        public FolderMonitoringService(IMessageQueueService msgQService, IOptions<AppConfig> cfg, ILogger<FolderMonitoringService> logger, IDistributedSetRepo repo)
        {
            _msgQService = msgQService;
            _logger = logger;
            _repo = repo;
            _subscriptionId = cfg.Value.MessageQueueConfig.FileEventSubscriptionId;
            var bufferSizeKb = cfg.Value.FolderMonitoringConfig.InternalBufferSizeKb;
            _watcherBufferSize = GetVerifiedBufferSizeInBytes(bufferSizeKb);
            _eventInterceptor = ConfigureEventInterceptor(cfg.Value.FolderMonitoringConfig);
            _errorHandler = (o, e) => { _logger.LogError("Error Listening To Folder {o}: {e}", (o as FileSystemWatcher)?.Path, e.GetException()); };
        }

        public async Task<bool> StartMonitoring(string folderName)
        {
            _logger.LogInformation("Started Monitoring {folderName}", folderName);
            if (_watchers.ContainsKey(folderName)) return false;

            var watcher = new FileSystemWatcherWrapper(folderName, _watcherBufferSize, e => _eventInterceptor.Post(e), _errorHandler);
            _watchers[folderName] = watcher;
            try
            {
                watcher.StartWatch();
                await _repo.CreateAsync(folderName);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to start monitoring {folderName}: {ex}");
            }
        }

        public async Task<bool> StopMonitoring(string folderName)
        {
            _logger.LogInformation("Stopped Monitoring {folderName}", folderName);
            if (!_watchers.TryGetValue(folderName, out var watcher)) return false;
            try
            {
                watcher.StopWatch();
                watcher.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed Stopping watch for {folderName}: {msg}", folderName, ex.Message);
            }
            finally
            {
                await _repo.DeleteAsync(folderName);
            }
            return true;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting FolderMonitoringService");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping FolderMonitoringService");
            foreach (var folderName in _watchers.Keys)
            {
                await StopMonitoring(folderName);
            }
        }

        private void ActionSendMsg(FileSystemEventArgs e)
        {
            var msgToSend = new FileEventEmittedMessage
            {
                ChangeType = e.ChangeType.ToString(),
                FullPath = e.FullPath,
                EventDate = DateTime.UtcNow
            };
            _msgQService.Send(_subscriptionId, msgToSend);
        }

        private ActionBlock<FileSystemEventArgs> ConfigureEventInterceptor(FolderMonitoringConfig cfg)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = cfg.EventInterceptorCfg.MaxParallelism,
                BoundedCapacity = cfg.EventInterceptorCfg.MaxMessagesPerBuffer,
            };
            return new ActionBlock<FileSystemEventArgs>(ActionSendMsg, options);
        }

        private static int GetVerifiedBufferSizeInBytes(int cfgBufferSize)
        {
            ;
            var verified = Const.FileSysWatcher.BufferMultiplierKb > 1 &&
                           cfgBufferSize is >= Const.FileSysWatcher.MinBufferSizeKb and <= Const.FileSysWatcher.MaxBufferSizeKb
                           && cfgBufferSize % Const.FileSysWatcher.BufferMultiplierKb == 0;
            if (!verified)
            {
                throw new ArgumentException($"Configuration Buffer size {cfgBufferSize} is invalid. Value must be 4-64 and a multiple of 4.");
            }

            return cfgBufferSize * 1024;
        }

    }

}
