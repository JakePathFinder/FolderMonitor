using Common.Services.Interfaces;
using FileListener.Constants;
using FileListener.Repos.Interfaces;

namespace FileListenerTests.Services
{
    public class FolderMonitoringServiceTests
    {
        private readonly ILogger<FolderMonitoringService> _logger = Substitute.For<ILogger<FolderMonitoringService>>();
        private readonly IUtilitiesService _utilities = Substitute.For<IUtilitiesService>();
        private readonly IOptions<AppConfig> _config = Substitute.For<IOptions<AppConfig>>();
        private readonly IMessageQueueService _mqService = Substitute.For<IMessageQueueService>();
        private readonly IDistributedSetRepo _repo = Substitute.For<IDistributedSetRepo>();
        private readonly RedisConfig _redisConfig = new() { FolderCollectionName = "Test" };
        private readonly MessageQueueConfig _rmqConfig = new() { FileEventSubscriptionId = "Test" };
        private const string FolderName = "TestFolder";

        [Fact]
        public void Constructor_ThrowsException_WhenBufferOver64Kb()
        {
            const int size = Const.FileSysWatcher.MaxBufferSizeKb +
                             1 * Const.FileSysWatcher.BufferMultiplierKb;

            _config.Value.Returns(BuildCfg(size));

            Assert.Throws<ArgumentException>(() => new FolderMonitoringService(_mqService, _config, _logger, _repo));
        }

        [Fact]
        public void Constructor_ThrowsException_WhenBufferIsNotMultipleOfBufferMultiplierKb()
        {
            const int size = Const.FileSysWatcher.MinBufferSizeKb +
                Const.FileSysWatcher.BufferMultiplierKb - 1;

            _config.Value.Returns(BuildCfg(size));

            Assert.Throws<ArgumentException>(() => new FolderMonitoringService(_mqService, _config, _logger, _repo));
        }

        [Fact]
        public async void StopMonitoring_ValueExists_ReturnsTrue()
        {
            _config.Value.Returns(BuildCfg());
            _utilities.IsValidFolder(FolderName).Returns(true);

            var service = new FolderMonitoringService(_mqService, _config, _logger, _repo);
            await service.StartMonitoring(FolderName);

            var result = await service.StopMonitoring(FolderName);

            Assert.True(result);
        }

        [Fact]
        public async void StopMonitoring_ValueDoesNotExist_ReturnsFalse()
        {
            _config.Value.Returns(BuildCfg());
            var service = new FolderMonitoringService(_mqService, _config, _logger, _repo);
            var result = await service.StopMonitoring(FolderName);

            Assert.False(result);
        }

        [Fact]
        public async void StopAsync_StopsMonitoring()
        {
            _config.Value.Returns(BuildCfg());
            _utilities.IsValidFolder(FolderName).Returns(true);

            var service = new FolderMonitoringService(_mqService, _config, _logger, _repo);
            await service.StartMonitoring(FolderName);

            await service.StopAsync(CancellationToken.None);
            var result = await service.StopMonitoring(FolderName);

            Assert.False(result);
        }

        private AppConfig BuildCfg(int internalBufferSizeKb = 8)
        {
            return new AppConfig
            {
                FolderMonitoringConfig = new FolderMonitoringConfig
                {
                    InternalBufferSizeKb = internalBufferSizeKb,
                    MaxAllowedFolders = 5,
                    EventInterceptorCfg = new EventInterceptorCfg()
                    {
                        MaxMessagesPerBuffer = 1,
                        MaxParallelism = 1,
                    }
                },
                RedisConfig = _redisConfig,
                MessageQueueConfig = _rmqConfig
            };
        }
    }
}
