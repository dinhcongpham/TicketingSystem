using BookingService.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace BookingService.Application.Implementation
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
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
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

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                
                return await _db.StringSetAsync(key, json, expiry);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                return await _db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteMultipleAsync(IEnumerable<string> keys)
        {
            if (keys == null || !keys.Any())
                return true; // Nothing to delete is considered success

            try
            {
                var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
                var deleted = await _db.KeyDeleteAsync(redisKeys);

                // Return true if all keys were processed (some might not exist)
                return deleted >= 0; // KeyDeleteAsync returns count of actually deleted keys
            }
            catch (RedisException ex)
            {
                // Log Redis-specific errors
                throw; // Re-throw to let caller handle
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                throw;
            }
        }
    }
}
