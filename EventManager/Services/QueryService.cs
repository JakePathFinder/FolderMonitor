using EventManager.Cfg;
using EventManager.Model;
using EventManager.Repos.Interfaces;
using EventManager.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace EventManager.Services
{
    public class QueryService : IQueryService
    {
        private readonly IFileEventRepo _repo;
        private readonly int _maxNumEvents;

        public QueryService(IFileEventRepo repo, IOptions<AppConfig> cfg)
        {
            _repo = repo;
            _maxNumEvents = cfg.Value.QueryControllerConfig.MaxAllowedRecordsToReturn;
        }

        public Task<List<FileEvent>> QueryAllByDateAsync(int numEvents)
        {
            VerifyNumEvents(numEvents);
            return _repo.QueryLastEventsAsync(numEvents);
        }

        public Task<List<FileEvent>> QueryByFolderAsync(int numEvents, string? folderName = null)
        {
            VerifyNumEvents(numEvents);
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentException("Empty folder");
            }
            return _repo.QueryLastEventsAsync(numEvents, folderName:folderName);
        }

        public Task<List<FileEvent>> QueryByEventAsync(int numEvents, string? eventType = null)
        {
            VerifyNumEvents(numEvents);
            if (string.IsNullOrEmpty(eventType))
            {
                throw new ArgumentException($"Empty event {eventType}");
            }
            return _repo.QueryLastEventsAsync(numEvents, eventType:eventType);
        }

        private void VerifyNumEvents(int numEvents)
        {
            if (numEvents < 1 || numEvents > _maxNumEvents)
            {
                throw new ArgumentOutOfRangeException($"Invalid number of events. Must be between 1 and {_maxNumEvents}");
            }
        }
    }
}
