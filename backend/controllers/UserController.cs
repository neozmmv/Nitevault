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
using Microsoft.AspNetCore.Http.HttpResults;

[ApiController]
[Route("/api")]
public class UserController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly UserService _userService;
    private readonly AppDbContext _db;

    public UserController(JwtService jwtService, AppDbContext db, UserService userService)
    {
        _jwtService = jwtService;
        _db = db;
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<ActionResult> PostSign([FromBody] LoginRequest login)
    {
        string token = await _userService.Login(login);
        if(token.Length < 1) return Unauthorized(new {error = "Invalid login!"});
        return Ok(new{token});
    }

    // migrate to UserService.cs
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

    // migrate to UserService.cs
    [Authorize]
    [HttpGet("user/{id}")]
    public async Task<ActionResult> GetUser(Guid Id)
    {
        var tokenOwner = User.GetAuthorizedTokenOwner();
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