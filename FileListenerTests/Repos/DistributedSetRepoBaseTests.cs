using Common.Constants;
using Common.Exceptions;
using Common.Services.Interfaces;
using FileListener.Repos;
using NSubstitute.ExceptionExtensions;
using StackExchange.Redis;
using static NSubstitute.Arg;

namespace FileListenerTests.Repos
{
    public class TestDistributedSetRepo : DistributedSetRepoBase
    {
        public TestDistributedSetRepo(string key, IRedisConnectionFactory redis, ILogger<DistributedSetRepoBase> logger)
            : base(key, redis, logger)
        {
        }
    }

    public class DistributedSetRepoBaseTests
    {
        private const string DbKey = "key";
        private const string TestItem = "TestItem";
        private const string ExceptionMsg = "Exception Test Message";
        private readonly IDatabase _dbMock;
        private readonly ILogger<DistributedSetRepoBase> _loggerMock;
        private readonly IRedisConnectionFactory _redisConnectionFactoryMock;

        public DistributedSetRepoBaseTests()
        {
            _dbMock = Substitute.For<IDatabase>();
            _loggerMock = Substitute.For<ILogger<DistributedSetRepoBase>>();
            _redisConnectionFactoryMock = Substitute.For<IRedisConnectionFactory>();
            _redisConnectionFactoryMock.GetDatabase().Returns(_dbMock);
        }

        [Fact]
        public async Task CountAsync_ShouldReturnExpectedValue_WhenNoExceptionOccurs()
        {
            // Arrange
            const long expectedCount = 5;
            _dbMock.SetLengthAsync(Any<RedisKey>(), Any<CommandFlags>()).Returns(expectedCount);
            var repo = new TestDistributedSetRepo(DbKey, _redisConnectionFactoryMock, _loggerMock);

            // Act
            var result = await repo.CountAsync();

            // Assert
            Assert.Equal(expectedCount, result);
        }

        [Fact]
        public async Task CountAsync_ShouldLogErrorAndThrowRepoException_WhenExceptionOccurs()
        {
            var originalException = new Exception(ExceptionMsg);
            // Arrange
            _dbMock.SetLengthAsync(Any<RedisKey>(), Any<CommandFlags>()).Throws(originalException);
            var repo = new TestDistributedSetRepo(DbKey, _redisConnectionFactoryMock, _loggerMock);

            // Act
            var ex = await Record.ExceptionAsync(async () => await repo.CountAsync());

            // Assert
            AssertRepoException(ex, nameof(DistributedSetRepoBase.CountAsync));

            AssertLogCalledOnce();
        }

        private static void AssertRepoException(Exception? ex, string methodName)
        {
            Assert.NotNull(ex);
            Assert.IsType<RepoException>(ex);
            var expectedMessage = string.Format(Const.DbExceptionMessageTemplate, methodName, ExceptionMsg);
            Assert.Equal(expectedMessage, ex.Message);
        }

        private void AssertLogCalledOnce()
        {
            var actualNumLogCalls = _loggerMock.ReceivedCalls().ToList().Count;
            const int expectedNumLogCalls = 1;
            Assert.Equal(expected: expectedNumLogCalls, actual: actualNumLogCalls);
        }

        [Fact]
        public async Task ExistsAsync_ShouldCallDbSetContainsAsyncWithCorrectKeyAndItem()
        {
            // Arrange
            var repo = new TestDistributedSetRepo(DbKey, _redisConnectionFactoryMock, _loggerMock);

            // Act
            await repo.ExistsAsync(TestItem);

            // Assert
            await _dbMock.Received().SetContainsAsync(DbKey, TestItem);
        }

        [Fact]
        public async Task ExistsAsync_ShouldLogErrorAndThrowRepoException_WhenExceptionOccurs()
        {
            var originalException = new Exception(ExceptionMsg);

            // Arrange
            _dbMock.SetContainsAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Throws(originalException);
            var repo = new TestDistributedSetRepo(DbKey, _redisConnectionFactoryMock, _loggerMock);

            // Act & Assert
            var ex = await Record.ExceptionAsync(() => repo.ExistsAsync(TestItem));
            AssertRepoException(ex, nameof(DistributedSetRepoBase.ExistsAsync));

            AssertLogCalledOnce();
        }

        [Fact]
        public async Task CreateAsync_ShouldCallDbSetAddAsyncWithCorrectKeyAndItem()
        {
            // Arrange
            var repo = new TestDistributedSetRepo(DbKey, _redisConnectionFactoryMock, _loggerMock);

            // Act
            await repo.CreateAsync(TestItem);

            // Assert
            await _dbMock.Received().SetAddAsync(DbKey, TestItem);
        }

        [Fact]
        public async Task CreateAsync_ShouldLogErrorAndThrowRepoException_WhenExceptionOccurs()
        {
            var originalException = new Exception(ExceptionMsg);

            // Arrange
            _dbMock.SetAddAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Throws(originalException);
            var repo = new TestDistributedSetRepo(DbKey, _redisConnectionFactoryMock, _loggerMock);

            // Act & Assert
            var ex = await Record.ExceptionAsync(() => repo.CreateAsync(TestItem));
            AssertRepoException(ex, nameof(DistributedSetRepoBase.CreateAsync));

            AssertLogCalledOnce();
        }
    }

}
