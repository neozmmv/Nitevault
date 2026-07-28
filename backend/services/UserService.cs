using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using nitevault.Dto;
using nitevault.Models;
using System.Text;

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

    /// <summary>
    /// Gets LoginRequest from UserController and handles token generation
    /// </summary>
    /// <param name="login"></param>
    /// <returns>JWT Token</returns>
    public async Task<JWTToken> Login(LoginRequest login)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == login.email);
        if(user is null || !BCrypt.Net.BCrypt.Verify(login.password, user.PasswordHash)) throw new Exception("Invalid Login!");
        JWTToken jwt = _jwt.GenerateToken(user.Id.ToString(), user.Email);
        string refresh = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(jwt.refresh)));
        
        RefreshToken rt = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync();
        return jwt;
    }
}