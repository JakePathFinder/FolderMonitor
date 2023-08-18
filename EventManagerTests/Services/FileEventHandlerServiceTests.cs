using Common.DTO;
using Common.Services.Interfaces;
using EventManager.Cfg;
using EventManager.Repos.Interfaces;
using EventManager.Services;
using Microsoft.Extensions.Options;

namespace EventManagerTests.Services
{
    public class FileEventHandlerServiceTests
    {
        private const string MockSubscriptionId = "MockSubscriptionId";
        private readonly IOptions<AppConfig> _configMock = Substitute.For<IOptions<AppConfig>>();
        private readonly ILogger<FileEventHandlerService> _loggerMock = Substitute.For<ILogger<FileEventHandlerService>>();
        private readonly IFileEventRepo _repoMock = Substitute.For<IFileEventRepo>();
        private readonly IUtilitiesService _utilitiesMock = Substitute.For<IUtilitiesService>();

        public FileEventHandlerServiceTests()
        {
            _configMock.Value.Returns(BuildCfg());
        }

        [Fact]
        public async Task StartAsync_ShouldSubscribeWithCorrectSubscriptionId()
        {
            // Arrange
            var messageQueueService = Substitute.For<IMessageQueueService>();
            var service = new FileEventHandlerService(messageQueueService, _configMock, _repoMock, _utilitiesMock, _loggerMock);

            // Act
            await service.StartAsync(CancellationToken.None);

            // Assert
            messageQueueService.Received().Subscribe<FileEventEmittedMessage>(MockSubscriptionId, Arg.Any<Func<FileEventEmittedMessage, Task>>());
        }

        [Fact]
        public async Task StopAsync_ShouldUnsubscribeWithCorrectSubscriptionId()
        {
            // Arrange
            var messageQueueService = Substitute.For<IMessageQueueService>();
            var service = new FileEventHandlerService(messageQueueService, _configMock, _repoMock, _utilitiesMock, _loggerMock);

            // Act
            await service.StopAsync(CancellationToken.None);

            // Assert
            messageQueueService.Received().Unsubscribe(MockSubscriptionId);
        }

            
        private static AppConfig BuildCfg()
        {
            return new AppConfig
            {
                QueryControllerConfig = new QueryControllerConfig
                {
                    MaxAllowedRecordsToReturn = 100
                },
                MessageQueueConfig = new MessageQueueConfig
                {
                    FileEventSubscriptionId = MockSubscriptionId
                },
                RedisConfig = new RedisConfig
                {
                    EventByFolderSortedSetKey = "test",
                    EventByTimeSortedSetKey = "test",
                    EventByTypeSortedSetKey = "test",
                    EventDetailsHashCollectionKey = "test"
                }
            };
        }
    }
}
