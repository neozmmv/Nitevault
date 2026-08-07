using nitevault.Dto;
using nitevault.Models;
using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

public class AuthService
{
    private readonly JwtService _jwt;
    private readonly AppDbContext _db;
    public AuthService(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    /// <summary>
    /// Gets LoginRequest from UserController and handles token generation
    /// </summary>
    /// <param name="login"></param>
    /// <returns>JWT Token</returns>
    public async Task<JWTToken> Login(LoginRequest login)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == login.email);
        if(user is null || !BCrypt.Net.BCrypt.Verify(login.password, user.PasswordHash)) throw new InvalidCredentialException("Invalid Login!");
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

    public async Task<bool> ExistsUserWithEmail(string email)
    {
        return await _db.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<int> CreateUser(User user)
    {
        _db.Users.Add(user);
        return await _db.SaveChangesAsync();
    }

    public async Task<JWTToken> RefreshToken(string refreshToken)
    {
        string hashedToken = HashToken(refreshToken);

        var storedToken = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hashedToken);

        if (storedToken is null || storedToken.ExpiresAt < DateTime.UtcNow || storedToken.Revoked)
            throw new InvalidCredentialException("Invalid refresh token!");

        User user = storedToken.User;

        // invalidate used token
        storedToken.Revoked = true;

        JWTToken newToken = _jwt.GenerateToken(user.Id.ToString(), user.Email);

        RefreshToken rt = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(newToken.refresh),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync();

        return newToken;
    }

    private static string HashToken(string token)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}