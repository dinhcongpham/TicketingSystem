namespace BookingService.Application.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task<bool> ExistsAsync(string key);
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<bool> RemoveAsync(string key);
        Task<bool> DeleteMultipleAsync(IEnumerable<string> keys);
    }
}
