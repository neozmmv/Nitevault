using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nitevault.Dto;
using nitevault.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Authentication;


[ApiController]
[Route("/api")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AppDbContext _db;

    public UserController(AppDbContext db, UserService userService)
    {
        _db = db;
        _userService = userService;
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

    // migrate to UserService.cs
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

    [Authorize]
    [HttpGet("user/{id}")]
    public async Task<ActionResult> GetUser(Guid Id)
    {
        var tokenOwner = User.GetAuthorizedTokenOwner();
        if(Id != tokenOwner)
        {
            return Forbid();
        }
        var user = await _userService.GetUser(Id);
        return Ok(user);
    }

    [HttpGet("checkAuth")]
    [Authorize]
    public ActionResult CheckAuth()
    {
        return Ok(new{auth = true});
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult UserInfo()
    {
        // gets id from Authorize
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new {id});
    }
}