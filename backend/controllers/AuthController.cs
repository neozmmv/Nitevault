using Microsoft.AspNetCore.Mvc;
using nitevault.Dto;
using System.Security.Authentication;
using nitevault.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("/api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtService _jwt;
    private readonly AppDbContext _db;
    public AuthController(UserService userService, AppDbContext db, JwtService jwt)
    {
        _userService = userService;
        _db = db;
        _jwt = jwt;
    }
    [HttpGet]
    public async Task<ActionResult> Auth()
    {
        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<JWTToken>> PostSign([FromBody] LoginRequest login)
    {
        JWTToken jwt;
        try
        {
            jwt = await _userService.Login(login);
        } catch(InvalidCredentialException err)
        {
            return Unauthorized(new {error = err.Message});
        }
        Response.Cookies.Append("jwt", jwt.token, new CookieOptions
        {
           HttpOnly = true,
           Secure = true,
           SameSite = SameSiteMode.Strict,
           Expires = DateTime.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("refresh-token", jwt.refresh, new CookieOptions
        {
           HttpOnly = true,
           Secure = true,
           SameSite = SameSiteMode.Strict,
           Expires = DateTime.UtcNow.AddDays(7)
        });
        return Ok(new {message = "Login successful!"});
    }

    [HttpPost("signUp")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUser user)
    {
        var exists = await _userService.ExistsUserWithEmail(user.email);
        if(exists)
        {
            return Conflict(new {error = "Account with this email already exists!"});
        }

        User u = new User
        {
            Email = user.email,
            Name = user.name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.password)
        };

        await _userService.CreateUser(u);

        var response = new
        {
            u.Email,
            u.Name
        };
        return Ok(response);
    }

    // possibly would be better in a service, who knows?
    [HttpPost("refresh")]
    public async Task<ActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refresh-token"];
        if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();

        // is it really worth it to create a service for this?? i dont think so
        var storedToken = await _db.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash == Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken))));
        if (storedToken is null || storedToken.ExpiresAt < DateTime.UtcNow || storedToken.Revoked) return Unauthorized();
        
        User user = storedToken.User;

        // invalidate used token
        storedToken.Revoked = true;
        JWTToken newToken = _jwt.GenerateToken(user.Id.ToString(), user.Email);
        RefreshToken rf = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(newToken.refresh))),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _db.RefreshTokens.Add(rf);
        await _db.SaveChangesAsync();

        Response.Cookies.Append("refresh-token", newToken.refresh, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = rf.ExpiresAt
        });

        return Ok(new{token = newToken.refresh});
    }
}