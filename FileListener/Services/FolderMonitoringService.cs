using Common.Services.Interfaces;
using FileListener.Cfg;
using FileListener.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Threading.Tasks.Dataflow;
using Common.DTO;
using FileListener.Constants;

namespace FileListener.Services
{
    public class FolderMonitoringService : IFolderMonitoringService
    {
        private readonly IMessageQueueService _msgQService;
        private readonly ILogger<FolderMonitoringService> _logger;
        private readonly IUtilitiesService _utilities;
        private readonly Dictionary<string, CancellationTokenSource> _monitoringTokens = new();
        private readonly int _bufferSize;
        private readonly string _subscriptionId;
        private readonly FileSystemEventHandler _eventHandler;
        private readonly RenamedEventHandler _renameEventHandler;
        private readonly ErrorEventHandler _errorHandler;

        public FolderMonitoringService(IMessageQueueService msgQService, IOptions<AppConfig> cfg, ILogger<FolderMonitoringService> logger, IUtilitiesService utilities)
        {
            _msgQService = msgQService;
            _logger = logger;
            _utilities = utilities;
            var bufferSizeKb = cfg.Value.FolderMonitoringConfig.InternalBufferSizeKb;
            _bufferSize = GetVerifiedBufferSizeInBytes(bufferSizeKb);
            _subscriptionId = cfg.Value.MessageQueueConfig.FileEventSubscriptionId;

            var eventInterceptor = ConfigureEventInterceptor(cfg.Value.FolderMonitoringConfig.EventInterceptorCfg);
            _eventHandler = (o, e) => eventInterceptor.Post(e);
            _renameEventHandler = (o, e) => eventInterceptor.Post(e);
            _errorHandler = (o, e) => { _logger.LogError("Error Listening To Folder {o}: {e}", (o as FileSystemWatcher)?.Path ,e.GetException());};
        }

        public bool StartMonitoring(string folderName)
        {
            if (!_utilities.IsValidFolder(folderName))
            {
                return false;
            }
            _logger.LogInformation("Started Monitoring {folderName}", folderName);
            if (_monitoringTokens.ContainsKey(folderName)) return false;
            var cts = new CancellationTokenSource();
            _monitoringTokens[folderName] = cts;

            Task.Run(() => MonitorFolder(folderName), cts.Token);
            return true;
        }

        public bool StopMonitoring(string folderName)
        {
            _logger.LogInformation("Stopped Monitoring {folderName}", folderName);
            if (!_monitoringTokens.TryGetValue(folderName, out var cts)) return false;
            cts.Cancel();
            _monitoringTokens.Remove(folderName);
            return true;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting FolderMonitoringService");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping FolderMonitoringService");
            foreach (var folderName in _monitoringTokens.Keys)
            {
                StopMonitoring(folderName);
            }
            return Task.CompletedTask;
        }

        private void MonitorFolder(string folderName)
        {
            using var watcher = new FileSystemWatcher
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                InternalBufferSize = _bufferSize,
                NotifyFilter = Const.NotifyFilters,
                Path = folderName
            };

            watcher.Changed += _eventHandler;
            watcher.Created += _eventHandler;
            watcher.Deleted += _eventHandler;
            watcher.Renamed += _renameEventHandler;
            watcher.Error += _errorHandler;

            watcher.EnableRaisingEvents = true;
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

        private void ActionSendMsg(FileSystemEventArgs e)
        {
            var msgToSend = new FileEventEmittedMessage
            {
                EventArgs = e,
                HandledDateTimeUtc = DateTime.UtcNow
            };
            _msgQService.Send(_subscriptionId, msgToSend);
        }

        private ActionBlock<FileSystemEventArgs> ConfigureEventInterceptor(EventInterceptorCfg cfg)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = cfg.MaxParallelism,
                BoundedCapacity = cfg.MaxMessagesPerBuffer
            };
            return new ActionBlock<FileSystemEventArgs>(ActionSendMsg, options);
        }

    }

}
