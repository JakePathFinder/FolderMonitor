using Common.Repos.Interfaces;

namespace FileListener.Repos.Interfaces
{
    public interface IDistributedSetRepo : IRepo<string>
    {
        Task<List<string>> GetAllAsync();
    }
}
