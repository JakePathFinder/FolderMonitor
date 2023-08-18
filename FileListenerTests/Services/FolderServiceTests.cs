using Common.Repos.Interfaces;
using Common.Services.Interfaces;
using FileListener.Cfg;
using FileListener.Repos.Interfaces;
using FileListener.Services;
using FileListener.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileListenerTests.Services
{
    public class FolderServiceTests
    {
        private const int MaxFolders = 5;
        private const string TestFolderName = "testFolder";
        private readonly IFolderService _folderService;
        private readonly IUtilitiesService _utilities = Substitute.For<IUtilitiesService>();
        private readonly IDistributedSetRepo _repoMock = Substitute.For<IDistributedSetRepo>();
        private readonly IOptions<AppConfig> _configMock = Substitute.For<IOptions<AppConfig>>();
        private readonly IFolderMonitoringService _monitoringServiceMock = Substitute.For<IFolderMonitoringService>();

        public FolderServiceTests()
        {
            _configMock.Value.Returns(new AppConfig
            {
                FolderMonitoringConfig = new FolderMonitoringConfig
                {
                    MaxAllowedFolders = MaxFolders,
                    InternalBufferSizeKb = 8,
                    EventInterceptorCfg = new EventInterceptorCfg
                    {
                        MaxMessagesPerBuffer = 4, 
                        MaxParallelism = 10
                    }
                },
                RedisConfig = new RedisConfig
                {
                    FolderCollectionName = "Test",
                },
                MessageQueueConfig = new MessageQueueConfig
                {
                    FileEventSubscriptionId = "Test"
                }

            });
            _folderService = new FolderService(_monitoringServiceMock, _repoMock, _utilities, _configMock);
        }

        [Fact]
        public async Task AddFolderAsync_ThrowsException_WhenMaxFoldersExceeded()
        {
            // Arrange
            _utilities.IsValidFolder(TestFolderName).Returns(true);
            _repoMock.CountAsync().Returns(MaxFolders);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _folderService.AddFolderAsync(TestFolderName));
        }

        [Fact]
        public async Task AddFolderAsync_ShouldReturnTrue_WhenFolderExists()
        {
            // Arrange
            _utilities.IsValidFolder(TestFolderName).Returns(true);
            _monitoringServiceMock.StartMonitoring(Arg.Is(TestFolderName)).Returns(true);
            _repoMock.ExistsAsync(TestFolderName).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => await _folderService.AddFolderAsync(TestFolderName));
            await _repoMock.DidNotReceive().CreateAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task AddFolderAsync_ShouldReturnFalse_WhenStartMonitoringFails()
        {
            // Arrange
            _utilities.IsValidFolder(TestFolderName).Returns(true);
            _monitoringServiceMock.StartMonitoring(Arg.Is(TestFolderName)).Returns(false);
            _repoMock.ExistsAsync(TestFolderName).Returns(false);
            _repoMock.CountAsync().Returns(MaxFolders - 1);

            // Act
            var result = await _folderService.AddFolderAsync(TestFolderName);

            // Assert
            await _repoMock.DidNotReceive().CreateAsync(Arg.Any<string>());
            Assert.False(result);
        }

        [Fact]
        public async Task AddFolderAsync_ThrowsException_WhenFolderDoesNotExist()
        {
            // Arrange
            _utilities.IsValidFolder(TestFolderName).Returns(false);
            _monitoringServiceMock.StartMonitoring(Arg.Is(TestFolderName)).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await _folderService.AddFolderAsync(TestFolderName));
        }

        [Fact]
        public async void RemoveFolder_ShouldReturnTrue_WhenDeleteSucceeds()
        {
            // Arrange
            _repoMock.CountAsync().Returns(1);
            _utilities.IsValidFolder(TestFolderName).Returns(true);
            _monitoringServiceMock.StopMonitoring(Arg.Is(TestFolderName)).Returns(true);
            _repoMock.DeleteAsync(TestFolderName).Returns(true);

            // Act
            var result = await _folderService.RemoveFolderAsync(TestFolderName);

            // Assert
            await _repoMock.Received().DeleteAsync(TestFolderName);
            Assert.True(result);
        }

        [Fact]
        public async void RemoveFolder_ReturnsFalse_WhenDeleteFails()
        {
            // Arrange
            _repoMock.CountAsync().Returns(1);
            _utilities.IsValidFolder(TestFolderName).Returns(true);
            _monitoringServiceMock.StopMonitoring(Arg.Is(TestFolderName)).Returns(true);
            _repoMock.DeleteAsync(TestFolderName).Returns(false);

            // Act & Assert
            var result = await _folderService.RemoveFolderAsync(TestFolderName);
            await _repoMock.Received().DeleteAsync(TestFolderName);
            Assert.False(result);
        }



        [Fact]
        public async void RemoveFolder_ShouldReturnFalse_WhenStopMonitoringFails()
        {
            // Arrange
            _repoMock.CountAsync().Returns(1);
            _utilities.IsValidFolder(TestFolderName).Returns(true);
            _monitoringServiceMock.StopMonitoring(Arg.Is(TestFolderName)).Returns(false);
            _repoMock.DeleteAsync(TestFolderName).Returns(false);

            // Act
            var result = await _folderService.RemoveFolderAsync(TestFolderName);

            // Assert
            await _repoMock.DidNotReceive().DeleteAsync(TestFolderName);
            _monitoringServiceMock.Received().StopMonitoring(TestFolderName);
            Assert.False(result);
        }

        [Fact]
        public async Task AddFolderAsync_ThrowsException_WhenFolderIsNotValid()
        {
            // Arrange
            _utilities.IsValidFolder(TestFolderName).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _folderService.AddFolderAsync(TestFolderName));
        }

        [Fact]
        public async Task RemoveFolderAsync_ThrowsException_WhenFolderIsNotValid()
        {
            // Arrange
            _utilities.IsValidFolder(TestFolderName).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _folderService.RemoveFolderAsync(TestFolderName));
        }

        [Fact]
        public async Task RemoveFolderAsync_ThrowsException_WhenNoFoldersAreMonitored()
        {
            // Arrange
            _utilities.IsValidFolder(TestFolderName).Returns(true);
            _repoMock.CountAsync().Returns(0);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _folderService.RemoveFolderAsync(TestFolderName));
        }

        [Fact]
        public async Task AddFolderAsync_ThrowsException_WhenFolderAlreadyExists()
        {
            // Arrange
            _utilities.IsValidFolder(TestFolderName).Returns(true);
            _repoMock.ExistsAsync(TestFolderName).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _folderService.AddFolderAsync(TestFolderName));
        }

        [Fact]
        public async Task GetAllFoldersAsync_ReturnsExpectedFolders()
        {
            // Arrange
            var expectedFolders = new List<string> { "Folder1", "Folder2" };
            _repoMock.GetAllAsync().Returns(expectedFolders);

            // Act
            var result = await _folderService.GetAllFoldersAsync();

            // Assert
            Assert.Equal(expectedFolders, result);
        }
    }

}
