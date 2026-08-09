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
    [HttpGet("{userId}")]
    public async Task<ActionResult> GetUser(Guid userId)
    {
        var tokenOwner = User.GetAuthorizedTokenOwner();
        if(userId != tokenOwner)
        {
            return Forbid();
        }

        var user = await _userService.GetUser(userId);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult> UserInfo()
    {
        var userId = User.GetAuthorizedTokenOwner();
        UserDTO? user = await _userService.GetUser(userId);
        return Ok(user);
    }
}