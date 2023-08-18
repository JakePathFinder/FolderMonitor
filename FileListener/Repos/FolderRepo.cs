using Common.Services.Interfaces;
using FileListener.Cfg;
using Microsoft.Extensions.Options;

namespace FileListener.Repos
{
    public class FolderRepo : DistributedSetRepoBase
    {
        public FolderRepo(IOptions<AppConfig> cfg, IRedisConnectionFactory factory, ILogger<FolderRepo> logger)
            : base(cfg.Value.RedisConfig.FolderCollectionName, factory, logger)
        {
        }
    }
}
