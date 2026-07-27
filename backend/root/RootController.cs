using Microsoft.AspNetCore.Mvc;

// this is for testing
// IResult with Results static class
// IActionResult / ActionResult<T> -> auto schema infer

[ApiController]
[Route("/")]
public class RootController : ControllerBase
{
    [HttpGet]
    public IResult Root()
    {
        return Results.Ok(new
        {
            Hello = "World!"
        });
    }
}