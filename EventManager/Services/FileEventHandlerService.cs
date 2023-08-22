using Common.DTO;
using Common.Services.Interfaces;
using EventManager.Cfg;
using EventManager.Model;
using EventManager.Repos.Interfaces;
using Microsoft.Extensions.Options;

namespace EventManager.Services
{
    public class FileEventHandlerService : IHostedService
    {
        private readonly IMessageQueueService _msgQService;
        private readonly IFileEventRepo _repo;
        private readonly IUtilitiesService _utilities;
        private readonly ILogger<FileEventHandlerService> _logger;
        private readonly string _subscriptionId;

        public FileEventHandlerService(IMessageQueueService msgQService, IOptions<AppConfig> cfg, IFileEventRepo repo, IUtilitiesService utilities, ILogger<FileEventHandlerService> logger)
        {
            _msgQService = msgQService;
            _repo = repo;
            _utilities = utilities;
            _logger = logger;
            _subscriptionId = cfg.Value.MessageQueueConfig.FileEventSubscriptionId;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Subscribe(_subscriptionId, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _msgQService.Unsubscribe(_subscriptionId);
            return Task.CompletedTask;
        }

        private void Subscribe(string subscriptionId, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscribing to {subscriptionId}", subscriptionId);
            _msgQService.Subscribe<FileEventEmittedMessage>(subscriptionId,
                async fileEventMsg => { await DoWork(fileEventMsg, stoppingToken); });
        }

        private async Task DoWork(FileEventEmittedMessage fileEventMsg, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //Ignore folders
            if (_utilities.IsValidFolder(fileEventMsg.FullPath))
            {
                return;
            }

            try
            {
                var fileEvent = FileEvent.From(fileEventMsg);
                await _repo.CreateAsync(fileEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed intercepting message for {path}: {err}", fileEventMsg.FullPath, ex.Message);
            }
            
        }

        
    }
}
