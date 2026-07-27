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
    [HttpPost("login")]
    public ActionResult<User> PostLogin([FromBody] User user)
    {
        // login + jwt logic
        List<string> errors = new();
        if(user.password.Length < 5) errors.Add("Your password should be at least 5 chars long!");
        if (errors.Count > 0) return BadRequest(new{errors});
        return Ok(user);
    }
}