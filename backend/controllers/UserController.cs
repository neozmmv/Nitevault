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

    public UserController(UserService userService)
    {
        _userService = userService;
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