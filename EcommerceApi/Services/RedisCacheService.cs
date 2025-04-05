using System.Text.Json;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
        // Get cache timeout from environment variable, default to 30 if not set
        _cacheTimeout = int.Parse(Environment.GetEnvironmentVariable("REDIS_CACHE_TIMEOUT") ?? "30");
        _logger.LogInformation("RedisCacheService initialized with timeout: {Timeout} minutes", _cacheTimeout);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            if (!value.HasValue)
            {
                _logger.LogInformation("Cache miss for key: {Key}", key);
                return default;
            }
            
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving value for key: {Key}", key);
            throw;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var db = _redis.GetDatabase();
            var serializedValue = JsonSerializer.Serialize(value);
            var finalExpiry = expiry ?? TimeSpan.FromMinutes(_cacheTimeout);
            await db.StringSetAsync(key, serializedValue, finalExpiry);
            _logger.LogDebug("Successfully cached value for key: {Key} with expiry: {Expiry}", key, finalExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value for key: {Key}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
            _logger.LogInformation("Successfully removed key: {Key} from cache", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key: {Key} from cache", key);
            throw;
        }
    }
} 