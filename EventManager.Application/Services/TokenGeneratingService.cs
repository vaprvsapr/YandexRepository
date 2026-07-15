using EventManager.Domain.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventManager.Application.Services;

public class TokenGeneratingService
{
    private readonly string _jwtKey = "secret_key";

    public string GenerateToken(Guid userId, string login, UserRole role)
    {
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = userId.ToString(),
            ["login"] = login,
            ["role"] = role.ToString()
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Expires = DateTime.UtcNow.AddMinutes(15),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = creds
        };

        var tokenString = new JsonWebTokenHandler().CreateToken(descriptor);

        return tokenString;
    }
}
