using Microsoft.AspNetCore.Mvc;
using nitevault.Dto;
using System.Security.Authentication;
using nitevault.Models;

[ApiController]
[Route("/api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    public AuthController(UserService userService)
    {
        _userService = userService;
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
}