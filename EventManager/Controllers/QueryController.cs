using AutoMapper;
using EventManager.Constants;
using EventManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Controllers
{
    public class QueryController : Controller
    {
        private readonly IQueryService _service;
        private readonly IMapper _mapper;
        private readonly ILogger<QueryController> _logger;

        public QueryController(IQueryService service, IMapper mapper, ILogger<QueryController> logger)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet(nameof(LastEvents))]
        [SwaggerOperation(Summary = "Get Last Events By Date")]
        public async Task<List<DTO.FileEvent>> LastEvents([Required] int numEvents = Const.DefaultNumEventsToGet)
        {
            var results = await _service.QueryAllByDateAsync(numEvents);
            var resultsDto = _mapper.Map<List<DTO.FileEvent>>(results);
            return resultsDto;
        }

        [HttpGet(nameof(LastEventsByFolder))]
        [SwaggerOperation(Summary = "Get Last Events By Date")]
        public async Task<List<DTO.FileEvent>> LastEventsByFolder([Required] string folderName, [Required] int numEvents = Const.DefaultNumEventsToGet)
        {
            var results = await _service.QueryByFolderAsync(numEvents, folderName: folderName);
            var resultsDto = _mapper.Map<List<DTO.FileEvent>>(results);
            return resultsDto;
        }

        [HttpGet(nameof(LastEventsByEventType))]
        [SwaggerOperation(Summary = "Get Last Events By Event Type")]
        public async Task<List<DTO.FolderEvent>> LastEventsByEventType([Required] WatcherChangeTypes eventType, [Required] int numEvents = Const.DefaultNumEventsToGet)
        {
            var results = eventType == WatcherChangeTypes.All ? 
                await _service.QueryAllByDateAsync(numEvents) : 
                await _service.QueryByEventAsync(numEvents, eventType: eventType.ToString());
            var resultsDto = _mapper.Map<List<DTO.FolderEvent>>(results);
            return resultsDto;
        }

    }
}
