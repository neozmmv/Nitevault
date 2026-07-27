using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nitevault.Dto;
using nitevault.Models;
using BCrypt.Net;
using Microsoft.VisualBasic;

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
    public ActionResult PostSign([FromBody] LoginRequest login)
    {
        string token = "";
        if(login.email == "test" && login.password == "1234")
        {
            token = _jwtService.GenerateToken("id", login.email);
        }
        return Ok(new{token});
    }

    [HttpPost("signUp")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUser user)
    {
        User u = new User
        {
            Email = user.email,
            Name = user.name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.password)
        };
        _db.Add(u);
        await _db.SaveChangesAsync();
        return Ok(u);
    }

    [HttpGet("checkAuth")]
    [Authorize]
    public ActionResult CheckAuth()
    {
        return Ok(new{auth = true});
    }
}