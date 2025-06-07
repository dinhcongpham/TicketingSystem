using EventService.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace EventService.Application.Implementation
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _db.StringGetAsync(key);
                return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting cache for key {key}: {ex.Message}", ex);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _db.StringSetAsync(key, json, expiry);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error setting cache for key {key}: {ex.Message}", ex);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing cache for key {key}: {ex.Message}", ex);
            }
        }
    }
}
