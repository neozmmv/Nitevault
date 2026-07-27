using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nitevault.Dto;

[ApiController]
[Route("/api")]
public class UserController : ControllerBase
{
    private readonly JwtService _jwtService;

    public UserController(JwtService jwtService)
    {
        _jwtService = jwtService;
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

    [HttpGet("checkAuth")]
    [Authorize]
    public ActionResult CheckAuth()
    {
        return Ok(new{auth = true});
    }
}