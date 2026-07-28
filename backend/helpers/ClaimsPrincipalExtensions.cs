using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetAuthorizedTokenOwner(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);

        if (claim is null || !Guid.TryParse(claim.Value, out var id))
        {
            throw new UnauthorizedAccessException("Invalid Token!");
        }

        return id;
    }
}