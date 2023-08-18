using EventManager.Cfg;
using EventManager.Repos.Interfaces;
using EventManager.Services;
using EventManager.Model;
using Microsoft.Extensions.Options;

namespace EventManagerTests.Services
{
    public class QueryServiceTests
    {
        private const int MaxAllowedRecords = 10;
        private const string TestFolderName = "testFolder";
        private const string TestEventType = "testEvent";
        private readonly IFileEventRepo _repoMock = Substitute.For<IFileEventRepo>();
        private readonly IOptions<AppConfig> _configMock = Substitute.For<IOptions<AppConfig>>();
        private readonly QueryService _queryService;

        public QueryServiceTests()
        {

            _configMock.Value.Returns(BuildCfg());

            _queryService = new QueryService(_repoMock, _configMock);
        }

        private FileEvent GenerateMockFileEvent()
        {
            return new FileEvent
            {
                EventType = WatcherChangeTypes.Created.ToString(),
                EventDate = DateTime.UtcNow,
                FileName = "file.txt",
                FolderName = "/data/test"
            };
        }

        [Fact]
        public async Task QueryAllByDateAsync_ReturnsCorrectData_WhenNumEventsValid()
        {
            // Arrange
            var expectedEvents = new List<FileEvent> { GenerateMockFileEvent() };
            _repoMock.QueryLastEventsAsync(1).Returns(expectedEvents);

            // Act
            var result = await _queryService.QueryAllByDateAsync(1);

            // Assert
            Assert.Equal(expectedEvents, result);
        }

        [Fact]
        public async Task QueryByFolderAsync_ReturnsCorrectData_WhenParametersValid()
        {
            // Arrange
            var expectedEvents = new List<FileEvent> { GenerateMockFileEvent() };
            _repoMock.QueryLastEventsAsync(1, TestFolderName).Returns(expectedEvents);

            // Act
            var result = await _queryService.QueryByFolderAsync(1, TestFolderName);

            // Assert
            Assert.Equal(expectedEvents, result);
        }

        [Fact]
        public async Task QueryByEventAsync_ReturnsCorrectData_WhenParametersValid()
        {
            // Arrange
            var expectedEvents = new List<FileEvent> { GenerateMockFileEvent() };
            _repoMock.QueryLastEventsAsync(1, TestEventType).Returns(expectedEvents);

            // Act
            var result = await _queryService.QueryByEventAsync(1, TestEventType);

            // Assert
            Assert.Equal(expectedEvents, result);
        }

        [Fact]
        public void QueryAllByDateAsync_ThrowsException_WhenNumEventsInvalid()
        {
            // Arrange, Act & Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _queryService.QueryAllByDateAsync(MaxAllowedRecords + 1));
        }

        [Fact]
        public void QueryByFolderAsync_ThrowsException_WhenFolderNameEmpty()
        {
            // Arrange, Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => _queryService.QueryByFolderAsync(1, ""));
        }

        [Fact]
        public void QueryByEventAsync_ThrowsException_WhenEventTypeEmpty()
        {
            // Arrange, Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => _queryService.QueryByEventAsync(1, ""));
        }

        private AppConfig BuildCfg()
        {
            return new AppConfig
            {
                QueryControllerConfig = new QueryControllerConfig
                {
                    MaxAllowedRecordsToReturn = MaxAllowedRecords
                },
                MessageQueueConfig = new MessageQueueConfig
                {
                    FileEventSubscriptionId = "test"
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
