using FileListener.Controllers;
using FileListener.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileListenerTests.Controllers
{
    public class FolderControllerTests
    {
        private readonly IFolderService _mockFolderService;
        private readonly FolderController _controller;
        private const string TestFolderName = "testFolder";

        public FolderControllerTests()
        {
            _mockFolderService = Substitute.For<IFolderService>();
            _controller = new FolderController(_mockFolderService);
        }

        [Fact]
        public async void Add_ShouldReturnOk_WhenServiceReturnsTrue()
        {
            // Arrange
            _mockFolderService.AddFolderAsync(TestFolderName).Returns(true);

            // Act
            var result = await _controller.AddFolder(TestFolderName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void Add_ShouldReturnBadRequest_WhenServiceReturnsFalse()
        {
            // Arrange
            _mockFolderService.AddFolderAsync(TestFolderName).Returns(false);

            // Act
            var result = await _controller.AddFolder(TestFolderName);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async void Remove_ShouldReturnOk_WhenServiceReturnsTrue()
        {
            // Arrange
            _mockFolderService.RemoveFolderAsync(TestFolderName).Returns(true);

            // Act
            var result = await _controller.RemoveFolder(TestFolderName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void Remove_ShouldReturnBadRequest_WhenServiceReturnsFalse()
        {
            // Arrange
            _mockFolderService.RemoveFolderAsync(TestFolderName).Returns(false);

            // Act
            var result = await _controller.RemoveFolder(TestFolderName);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }

}