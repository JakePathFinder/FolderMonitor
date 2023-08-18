using Common.Constants;
using Common.Services.Interfaces;
using EventManager.Cfg;
using EventManager.Model;
using EventManager.Repos.Interfaces;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Globalization;

namespace EventManager.Repos
{
    public class FileEventRepo : IFileEventRepo
    {
        private readonly ILogger<FileEventRepo> _logger;
        private readonly IDatabase _db;
        private readonly string _eventDetailsHashCollectionKey;
        private readonly string _eventByFolderSortedSetKey;
        private readonly string _eventByTypeSortedSetKey;
        private readonly string _eventByTimeSortedSetKey;

        public FileEventRepo(IOptions<AppConfig> cfg, IRedisConnectionFactory factory, ILogger<FileEventRepo> logger)
        {
            _logger = logger;
            _db = factory.GetDatabase();
            _eventDetailsHashCollectionKey = cfg.Value.RedisConfig.EventDetailsHashCollectionKey;
            _eventByFolderSortedSetKey = cfg.Value.RedisConfig.EventByFolderSortedSetKey;
            _eventByTypeSortedSetKey = cfg.Value.RedisConfig.EventByTypeSortedSetKey;
            _eventByTimeSortedSetKey = cfg.Value.RedisConfig.EventByTimeSortedSetKey;
        }

        public async Task<bool> CreateAsync(FileEvent fileEvent)
        {
            var eventId = fileEvent.GenerateId();

            try
            {
                await AddToMainCollection(fileEvent, eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed Adding fileEvent [{eventId}]: {err}", eventId, ex.Message);
                // On Exception, we don't want to Index to the Sets
                return false;
            }

            return await IndexToSortedSets(fileEvent, eventId);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var actualEvent = await GetAsync(id);
            if (actualEvent == null)
            {
                return false;
            }

            bool success;
            try
            {
                success = await RemoveIndexFromSortedSets(id, actualEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed Deleting {eventId}: {err}", id, ex.Message);
                //Do not remove from main collection if failed removing the indices
                return false;
            }

            success &= await RemoveFromMainCollection(id);

            return success;
        }

        public async Task<List<FileEvent>> QueryLastEventsAsync(int numEvents, string? folderName = null, string? eventType = null)
        {
            string collectionKey;
            
            if (!string.IsNullOrEmpty(folderName))
            {
                collectionKey = BuildSortedEventByFolderKey(folderName);
            }
            else if (!string.IsNullOrEmpty(eventType))
            {
                collectionKey = BuildSortedEventByTypeKey(eventType);
            }
            else //Illegal state where both have value will be prevented by verification (defaults to all records anyway)
            {
                collectionKey = BuildSortedEventsKey();
            }

            var eventIds = _db.SortedSetRangeByRank(collectionKey, -numEvents, -1, Order.Descending);

            List<FileEvent> fileEvents = new();

            foreach (var eventId in eventIds)
            {
                var eventIdStr = eventId.ToString();
                var hashData = await _db.HashGetAllAsync(BuildEventDetailsKey(eventIdStr));

                var fileEvent = HashEntriesToFileEvent(hashData);
                fileEvents.Add(fileEvent);
            }

            return fileEvents;
        }

        private static FileEvent HashEntriesToFileEvent(HashEntry[] hashData)
        {
            var dict = hashData.ToDictionary(he => he.Name.ToString(), he => he.Value.ToString());

            return new FileEvent
            {
                FolderName = dict["FolderName"],
                FileName = dict["FileName"],
                EventType = dict["EventType"],
                EventDate = DateTime.ParseExact(dict["EventDate"], "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            };
        }

        private async Task<bool> RemoveFromMainCollection(string id)
        {
            try
            {
                return await _db.KeyDeleteAsync($"{_eventDetailsHashCollectionKey}:{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed Deleting {eventId}: {err}", id, ex.Message);
                return false;
            }
        }

        public async Task<FileEvent?> GetAsync(string id)
        {
            var hashFields = await _db.HashGetAllAsync($"{_eventDetailsHashCollectionKey}:{id}");
            var hashDict = hashFields.ToDictionary(he => he.Name.ToString(), he => he.Value.ToString());
            try
            {
                var fileEvent = new FileEvent
                {
                    FolderName = hashDict[nameof(FileEvent.FolderName)],
                    FileName = hashDict[nameof(FileEvent.FileName)],
                    EventType = hashDict[nameof(FileEvent.EventType)],
                    EventDate = DateTime.Parse(hashDict[nameof(FileEvent.EventDate)])
                };
                return fileEvent;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed Getting {id}: {err}",id, ex.Message);
                return null;
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _db.KeyExistsAsync($"{_eventDetailsHashCollectionKey}:{id}");
        }

        public async Task<long> CountAsync()
        {
            return await _db.HashLengthAsync(_eventDetailsHashCollectionKey);
        }

        private async Task AddToMainCollection(FileEvent fileEvent, string eventId)
        {
            HashEntry[] hashFields =
            {
                new HashEntry(nameof(FileEvent.FolderName), fileEvent.FolderName),
                new HashEntry(nameof(FileEvent.FileName), fileEvent.FileName),
                new HashEntry(nameof(FileEvent.EventType), fileEvent.EventType),
                new HashEntry(nameof(FileEvent.EventDate), fileEvent.EventDate.ToString(Const.EventDateToStringFormat))
            };

            await _db.HashSetAsync(BuildEventDetailsKey(eventId), hashFields);
        }

        private string BuildSortedEventsKey()
        {
            return _eventByTimeSortedSetKey;
        }

        private string BuildSortedEventByFolderKey(string folderName)
        {
            return $"{_eventByFolderSortedSetKey}:{folderName}";
        }

        private string BuildSortedEventByTypeKey(string eventType)
        {
            return $"{_eventByTypeSortedSetKey}:{eventType}";
        }

        private string BuildEventDetailsKey(string eventId)
        {
            return $"{_eventDetailsHashCollectionKey}:{eventId}";
        }

        private async Task<bool> IndexToSortedSets(FileEvent fileEvent, string eventId)
        {
            var timestamp = fileEvent.EventDate.Ticks;
            return await AddToSortedSet(BuildSortedEventsKey(), eventId, timestamp)
                   & await AddToSortedSet(BuildSortedEventByFolderKey(fileEvent.FolderName), eventId, timestamp)
                   & await AddToSortedSet(BuildSortedEventByTypeKey(fileEvent.EventType), eventId, timestamp);
        }

        private async Task<bool> RemoveIndexFromSortedSets(string eventId, FileEvent fileEvent)
        {
            var success = await _db.SortedSetRemoveAsync(BuildSortedEventsKey(), eventId)
                       & await _db.SortedSetRemoveAsync(BuildSortedEventByFolderKey(fileEvent.FolderName), eventId)
                       & await _db.SortedSetRemoveAsync(BuildSortedEventByTypeKey(fileEvent.EventType), eventId);
            return success;
        }

        private async Task<bool> AddToSortedSet(string key, string eventId, long timestamp)
        {
            try
            {
                return await _db.SortedSetAddAsync(key, eventId, timestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed To Add {eventId} By Date: {err}", eventId, ex.Message);
                return false;
            }
        }
    }
}
