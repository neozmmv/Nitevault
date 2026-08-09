using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nitevault.Dto;
using System.Security.Claims;

[ApiController]
[Route("/api/user")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly RedisService _redis;

    public UserController(UserService userService, RedisService redis)
    {
        _userService = userService;
        _redis = redis;
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUser(Guid Id)
    {
        var tokenOwner = User.GetAuthorizedTokenOwner();
        if(Id != tokenOwner)
        {
            return Forbid();
        }

        var user = await _userService.GetUser(Id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
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