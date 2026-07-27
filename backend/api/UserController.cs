using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api")]
public class UserController : ControllerBase
{
    [HttpGet("test")]
    public IResult GetUser()
    {
        return Results.Ok(new
        {
            Name = "Test"
        });
    }
    
}