using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nitevault.Dto;
using nitevault.Models;
using Microsoft.EntityFrameworkCore;


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
        JWTToken jwt = await _userService.Login(login);
        if(jwt.token.Length < 1) return Unauthorized(new {error = "Invalid login!"});
        return Ok(jwt);
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