using StackExchange.Redis;
using System.Text.Json;

public class RedisService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisService> _logger;

    public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task SetObjectAsync<T>(string key, T value, Expiration expiry)
    {
        try
        {
            string json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiry);    
        } catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to read from Redis cache for key {Key}", key);
        }
    }

    public async Task<T?> GetObjectAsync<T>(string key)
    {
        try
        {
            RedisValue value = await _db.StringGetAsync(key);
            if(!value.HasValue) return default;
            string json = value!;
            return JsonSerializer.Deserialize<T>(json);    
        } catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to read from Redis cache for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync(string key, string value, Expiration expiry)
    {
        await _db.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetAsync(string key)
    {
        return await _db.StringGetAsync(key);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }
}