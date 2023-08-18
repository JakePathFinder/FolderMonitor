using EventManager.DTO;
using ModelFileEvent = EventManager.Model.FileEvent;
using DTOFileEvent = EventManager.DTO.FileEvent;

namespace EventManagerTests.Controllers
{
    public class QueryControllerTests
    {
        private readonly IQueryService _serviceMock = Substitute.For<IQueryService>();
        private readonly IMapper _mapperMock = Substitute.For<IMapper>();
        private readonly ILogger<QueryController> _loggerMock = Substitute.For<ILogger<QueryController>>();
        private QueryController _controller;

        public QueryControllerTests()
        {
            _controller = new QueryController(_serviceMock, _mapperMock, _loggerMock);
        }

        [Fact]
        public async Task LastEvents_ReturnsExpectedEvents()
        {
            // Arrange
            var numEvents = 5;
            var serviceResult = new List<ModelFileEvent> { CreateModelFileEvent() };
            var expectedResult = new List<DTOFileEvent> { CreateDTOFileEvent() };
            _serviceMock.QueryAllByDateAsync(numEvents).Returns(serviceResult);
            _mapperMock.Map<List<DTOFileEvent>>(serviceResult).Returns(expectedResult);

            // Act
            var result = await _controller.LastEvents(numEvents);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LastEventsByFolder_ReturnsExpectedEvents()
        {
            // Arrange
            var folderName = "testFolder";
            var numEvents = 5;
            var serviceResult = new List<ModelFileEvent> { CreateModelFileEvent() };
            var expectedResult = new List<DTOFileEvent> { CreateDTOFileEvent() };
            _serviceMock.QueryByFolderAsync(numEvents, folderName).Returns(serviceResult);
            _mapperMock.Map<List<DTOFileEvent>>(serviceResult).Returns(expectedResult);

            // Act
            var result = await _controller.LastEventsByFolder(folderName, numEvents);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LastEventsByEventType_ReturnsExpectedEventsForAll()
        {
            // Arrange
            var eventType = WatcherChangeTypes.All;
            var numEvents = 5;
            var serviceResult = new List<ModelFileEvent> { CreateModelFileEvent() };
            var expectedResult = new List<FolderEvent> { CreateFolderEvent() };
            _serviceMock.QueryAllByDateAsync(numEvents).Returns(serviceResult);
            _mapperMock.Map<List<FolderEvent>>(serviceResult).Returns(expectedResult);

            // Act
            var result = await _controller.LastEventsByEventType(eventType, numEvents);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LastEventsByEventType_ReturnsExpectedEventsForSpecificType()
        {
            // Arrange
            var eventType = WatcherChangeTypes.Created; // or any other specific event type
            var numEvents = 5;
            var serviceResult = new List<ModelFileEvent> { CreateModelFileEvent() };
            var expectedResult = new List<FolderEvent> { CreateFolderEvent() };
            _serviceMock.QueryByEventAsync(numEvents, eventType.ToString()).Returns(serviceResult);
            _mapperMock.Map<List<FolderEvent>>(serviceResult).Returns(expectedResult);

            // Act
            var result = await _controller.LastEventsByEventType(eventType, numEvents);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        private static ModelFileEvent CreateModelFileEvent()
        {
            return new ModelFileEvent
            {
                EventType = WatcherChangeTypes.Created.ToString(), EventDate = DateTime.UtcNow, FileName = "file.txt",
                FolderName = "/data/test"
            };
        }

        private static DTOFileEvent CreateDTOFileEvent()
        {
            return new DTOFileEvent
            {
                EventType = WatcherChangeTypes.Created.ToString(), EventDate = DateTime.UtcNow, FileName = "file.txt",
                FolderName = "/data/test"
            };
        }

        private static FolderEvent CreateFolderEvent()
        {
            return new FolderEvent
            {
                EventType = WatcherChangeTypes.Created.ToString(), EventDate = DateTime.UtcNow, FileName = "file.txt"
            };
        }
    }
}
