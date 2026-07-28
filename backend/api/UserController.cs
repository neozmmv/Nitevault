using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nitevault.Dto;
using nitevault.Models;
using BCrypt.Net;
using Microsoft.VisualBasic;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("/api")]
public class UserController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly AppDbContext _db;

    public UserController(JwtService jwtService, AppDbContext db)
    {
        _jwtService = jwtService;
        _db = db;
    }

    [HttpGet("user")]
    public ActionResult GetUser([FromQuery] string? name)
    {
        return Ok(new
        {
            Name = "Test",
            Query = name
        });
    }

    // TESTING
    // LOGIN ROUTE
    [HttpPost("login")]
    public async Task<ActionResult> PostSign([FromBody] LoginRequest login)
    {
        User user = await _db.Users.FirstOrDefaultAsync(u => u.Email == login.email);
        if(!BCrypt.Net.BCrypt.Verify(login.password, user.PasswordHash))
        {
            return Unauthorized(new {error = "Invalid login!"});
        }
        string token = _jwtService.GenerateToken(user.Id.ToString()!, user.Email);
        return Ok(new{token});
    }

    [HttpPost("signUp")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUser user)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == user.email);
        if(existing != null)
        {
            return Conflict(new {error = "Account with this email already exists!"});
        }

        User u = new User
        {
            Email = user.email,
            Name = user.name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.password)
        };

        _db.Add(u);
        await _db.SaveChangesAsync();
        var response = new
        {
            u.Email,
            u.Name
        };
        return Ok(response);
    }

    [Authorize]
    [HttpGet("user/{id}")]
    public async Task<ActionResult> GetAllUsers(Guid Id)
    {
        var tokenOwner = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if(Id != tokenOwner)
        {
            return Forbid();
        }
        var user = await _db.Users.FindAsync(Id);
        if(user is null) return NotFound();
        return Ok(user);
    }

    [HttpGet("checkAuth")]
    [Authorize]
    public ActionResult CheckAuth()
    {
        return Ok(new{auth = true});
    }
}