using Common.Constants;
using Common.Services.Interfaces;
using EventManager.Cfg;
using EventManager.Repos;
using EventManager.Model;
using Microsoft.Extensions.Options;
using NSubstitute.ExceptionExtensions;
using StackExchange.Redis;

namespace EventManagerTests.Repos
{
    public class FileEventRepoTests
    {
        private const string EventId = "testEventId";
        private readonly IDatabase _dbMock = Substitute.For<IDatabase>();
        private readonly IOptions<AppConfig> _configMock = Substitute.For<IOptions<AppConfig>>();
        private readonly ILogger<FileEventRepo> _loggerMock = Substitute.For<ILogger<FileEventRepo>>();
        private readonly IRedisConnectionFactory _factoryMock = Substitute.For<IRedisConnectionFactory>();
        private readonly FileEventRepo _fileEventRepo;

        public FileEventRepoTests()
        {
            _configMock.Value.Returns(BuildCfg());

            _factoryMock.GetDatabase().Returns(_dbMock);
            _fileEventRepo = new FileEventRepo(_configMock, _factoryMock, _loggerMock);
        }

        private FileEvent GenerateMockFileEvent()
        {
            return new FileEvent
            {
                FolderName = "testFolder",
                FileName = "testFile",
                EventType = "testType",
                EventDate = DateTime.Now
            };
        }

        [Fact]
        public async Task CreateAsync_ReturnsFalse_WhenExceptionOccurs()
        {
            // Arrange
            var fileEvent = GenerateMockFileEvent();
            _dbMock.HashSetAsync(Arg.Any<RedisKey>(), Arg.Any<HashEntry[]>()).Throws(new Exception("Test Exception"));

            // Act
            var result = await _fileEventRepo.CreateAsync(fileEvent);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenEventNotFound()
        {
            // Arrange
            _dbMock.HashGetAllAsync(Arg.Any<RedisKey>()).Returns(Array.Empty<HashEntry>());

            // Act
            var result = await _fileEventRepo.DeleteAsync(EventId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenEventDeleted()
        {
            // Arrange
            var mockFileEvent = GenerateMockFileEvent();
            var hashEntries = new[]
            {
                new HashEntry(nameof(FileEvent.FolderName), mockFileEvent.FolderName),
                new HashEntry(nameof(FileEvent.FileName), mockFileEvent.FileName),
                new HashEntry(nameof(FileEvent.EventType), mockFileEvent.EventType),
                new HashEntry(nameof(FileEvent.EventDate), mockFileEvent.EventDate.ToString())
            };
            _dbMock.HashGetAllAsync(Arg.Any<RedisKey>()).Returns(hashEntries);
            _dbMock.KeyDeleteAsync(Arg.Any<RedisKey>()).Returns(true);
            _dbMock.SortedSetRemoveAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(true);

            // Act
            var result = await _fileEventRepo.DeleteAsync(EventId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetAsync_ReturnsNull_WhenEventNotFound()
        {
            // Arrange
            _dbMock.HashGetAllAsync(Arg.Any<RedisKey>()).Returns(Array.Empty<HashEntry>());

            // Act
            var result = await _fileEventRepo.GetAsync(EventId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_ReturnsFileEvent_WhenEventFound()
        {
            // Arrange
            var mockFileEvent = GenerateMockFileEvent();
            var hashEntries = new[]
            {
                new HashEntry(nameof(FileEvent.FolderName), mockFileEvent.FolderName),
                new HashEntry(nameof(FileEvent.FileName), mockFileEvent.FileName),
                new HashEntry(nameof(FileEvent.EventType), mockFileEvent.EventType),
                new HashEntry(nameof(FileEvent.EventDate), mockFileEvent.EventDate.ToString(Const.EventDateToStringFormat))
            };
            _dbMock.HashGetAllAsync(Arg.Any<RedisKey>()).Returns(hashEntries);

            // Act
            var result = await _fileEventRepo.GetAsync(EventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockFileEvent.FolderName, result.FolderName);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenEventExists()
        {
            // Arrange
            _dbMock.KeyExistsAsync(Arg.Any<RedisKey>()).Returns(true);

            // Act
            var result = await _fileEventRepo.ExistsAsync(EventId);

            // Assert
            Assert.True(result);
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
