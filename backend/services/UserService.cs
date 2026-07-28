using Microsoft.EntityFrameworkCore;
using nitevault.Dto;
using nitevault.Models;

public class UserService
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    public UserService(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    /// <summary>
    /// Gets LoginRequest from UserController and handles token generation
    /// </summary>
    /// <param name="login"></param>
    /// <returns>JWT Token</returns>
    public async Task<string> Login(LoginRequest login)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == login.email);
        if(user is null) return string.Empty;
        if(BCrypt.Net.BCrypt.Verify(login.password, user.PasswordHash))
        {
            return _jwt.GenerateToken(user.Id.ToString(), user.Email);
        } else return string.Empty;
    }
}