using nitevault.Dto;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class JwtService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(IConfiguration config)
    {
        _key = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT_KEY MUST BE SET IN .env!");
        _issuer = "nitevault";
        _audience = "nitevault-users";
    }

    public JWTToken GenerateToken(string userId, string email)
    {
        List<Claim> claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        // refresh token
        byte[] random = new byte[64];
        RandomNumberGenerator.Fill(random);
        string refresh = Convert.ToBase64String(random);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new JWTToken(tokenString, refresh);
    }

    public string GenerateRefreshToken()
    {
        byte[] random = new byte[64];
        RandomNumberGenerator.Fill(random);
        string refresh = Convert.ToBase64String(random);
        return refresh;
    }
}