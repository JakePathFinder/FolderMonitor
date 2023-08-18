using Common.Exceptions;
using Common.Services.Interfaces;
using FileListener.Repos.Interfaces;
using StackExchange.Redis;
using static Common.Constants.Const;

namespace FileListener.Repos
{
    public abstract class DistributedSetRepoBase : IDistributedSetRepo
    {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly ILogger<DistributedSetRepoBase> _logger;

        protected DistributedSetRepoBase(string key, IRedisConnectionFactory factory, ILogger<DistributedSetRepoBase> logger)
        {
            _db = factory.GetDatabase();
            _logger = logger;
            _key = key;
        }

        public virtual async Task<long> CountAsync()
        {
            try
            {
                return await _db.SetLengthAsync(_key);
            }
            catch (Exception ex)
            {
                _logger.LogError(DbExceptionLoggerTemplate, nameof(CountAsync), ex);
                throw new RepoException(inner: ex, opName: nameof(CountAsync));
            }
        }

        public virtual async Task<bool> ExistsAsync(string item)
        {
            try
            {
                return await _db.SetContainsAsync(_key, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(DbExceptionLoggerTemplate, nameof(ExistsAsync), ex);
                throw new RepoException(inner: ex, opName: nameof(ExistsAsync));
            }
            
        }

        public virtual async Task<bool> CreateAsync(string item)
        {
            try
            {
                return await _db.SetAddAsync(_key, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(DbExceptionLoggerTemplate, nameof(CreateAsync), ex);
                throw new RepoException(inner: ex, opName: nameof(CreateAsync));
            }
        }

        public virtual async Task<bool> DeleteAsync(string item)
        {
            try
            {
                return await _db.SetRemoveAsync(_key, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(DbExceptionLoggerTemplate, nameof(DeleteAsync), ex);
                throw new RepoException(inner: ex, opName: nameof(DeleteAsync));
            }
            
        }

        public async Task<List<string>> GetAllAsync()
        {
            var values = await _db.SetMembersAsync(_key);
            return values.Select(value => value.ToString()).ToList();
        }
    }
}
