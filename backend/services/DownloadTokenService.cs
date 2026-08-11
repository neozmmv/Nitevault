using System.Security.Cryptography;

public class DownloadTokenService
{
    private readonly RedisService _redis;

    public DownloadTokenService(RedisService redis)
    {
        _redis = redis;
    }

    public async Task<string> GenerateToken(Guid fileId, Guid userId)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var key = $"download-token:{token}";
        var value = $"{fileId}:{userId}";

        await _redis.SetAsync(key, value, TimeSpan.FromMinutes(2));

        return token;
    }

    public async Task<(Guid fileId, Guid userId)?> ValidateToken(string token)
    {
        var key = $"download-token:{token}";
        var value = await _redis.GetAsync(key);

        if (string.IsNullOrEmpty(value)) return null;

        var parts = value.ToString().Split(':');
        var fileId = Guid.Parse(parts[0]);
        var userId = Guid.Parse(parts[1]);

        await _redis.DeleteAsync(key);

        return (fileId, userId);
    }
}