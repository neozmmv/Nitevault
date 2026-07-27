using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api")]
public class UserController : ControllerBase
{
    [HttpGet("user")]
    public IResult GetUser([FromQuery] string? name)
    {
        return Results.Ok(new
        {
            Name = "Test",
            Query = name
        });
    }
    
    // testing query and body
    [HttpPost("user")]
    public IActionResult PostUser([FromBody] User user, [FromQuery] string? q)
    {
        return Ok(new {user, q});
    }
}