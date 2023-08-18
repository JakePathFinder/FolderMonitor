namespace Common.Repos.Interfaces
{
    public interface IRepo<T>
    {
        Task<long> CountAsync();
        Task<bool> ExistsAsync(string id);
        Task<bool> CreateAsync(T item);
        Task<bool> DeleteAsync(string id);
    }
}
