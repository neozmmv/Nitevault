using StackExchange.Redis;
using System.Text.Json;

public class RedisService
{
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<bool> SetObjectAsync<T>(string key, T value, Expiration expiry)
    {
        string json = JsonSerializer.Serialize(value);
        return await _db.StringSetAsync(key, json, expiry);
    }

    public async Task<T?> GetObjectAsync<T>(string key)
    {
        RedisValue value = await _db.StringGetAsync(key);
        if(!value.HasValue) return default;
        string json = value!;
        return JsonSerializer.Deserialize<T>(json);
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