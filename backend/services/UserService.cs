using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using nitevault.Dto;
using nitevault.Models;
using System.Text;
using System.Security.Authentication;
public class UserService
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    private readonly RedisService _redis;
    public UserService(AppDbContext db, JwtService jwt, RedisService redis)
    {
        _db = db;
        _jwt = jwt;
        _redis = redis;
    }

    public async Task<UserDTO?> GetUser(Guid Id)
    {
        // try redis cache
        string cacheKey = $"user:{Id}";
        var cachedUser = await _redis.GetObjectAsync<UserDTO>(cacheKey);
        
        if (cachedUser is not null) {
            return cachedUser;
        }

        var user = await _db.Users.FindAsync(Id);
        if(user is null) return null;
        
        UserDTO userToCache = new UserDTO(
            Id: user.Id,
            Email: user.Email,
            Name: user.Name,
            CreatedAt: user.CreatedAt,
            Active: user.Active
        );

        await _redis.SetObjectAsync<UserDTO>(cacheKey, userToCache, TimeSpan.FromMinutes(5));
        return userToCache;
    }
}