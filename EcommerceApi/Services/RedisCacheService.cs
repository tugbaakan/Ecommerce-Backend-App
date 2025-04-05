using System.Text.Json;
using StackExchange.Redis;

namespace EcommerceApi.Services;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
}

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly int _cacheTimeout;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        // Get cache timeout from environment variable, default to 30 if not set
        _cacheTimeout = int.Parse(Environment.GetEnvironmentVariable("REDIS_CACHE_TIMEOUT") ?? "30");
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var db = _redis.GetDatabase();
        var serializedValue = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, serializedValue, expiry ?? TimeSpan.FromMinutes(_cacheTimeout));
    }

    public async Task RemoveAsync(string key)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
} 