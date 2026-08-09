using Microsoft.AspNetCore.Mvc;
using nitevault.Dto;
using System.Security.Authentication;
using nitevault.Models;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("/api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    public AuthController(AuthService auth)
    {
        _auth = auth;
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
            jwt = await _auth.Login(login);
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
        var exists = await _auth.ExistsUserWithEmail(user.email);
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

        await _auth.CreateUser(u);

        var response = new
        {
            u.Email,
            u.Name
        };
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refresh-token"];
        if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();

        JWTToken newToken;
        try
        {
            newToken = await _auth.RefreshToken(refreshToken);
        }
        catch (InvalidCredentialException)
        {
            return Unauthorized();
        }

        Response.Cookies.Append("jwt", newToken.token, new CookieOptions
        {
           HttpOnly = true,
           Secure = true,
           SameSite = SameSiteMode.Strict,
           Expires = DateTime.UtcNow.AddMinutes(15) 
        });

        Response.Cookies.Append("refresh-token", newToken.refresh, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(new {message = "Successfully refreshed the session."});
    }

    [HttpGet("checkAuth")]
    [Authorize]
    public ActionResult CheckAuth()
    {
        return Ok(new{auth = true});
    }
}