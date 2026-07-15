using EventManager.Domain.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventManager.Application.Services;

public static class TokenGeneratingService
{
    private static readonly string _jwtKey = "asdfjkasdjf83u8efjaisdjf8f3ue8fiaj8ef38EF38YUF3f33F33F";

    public static string GenerateToken(Guid userId, string login, UserRole role)
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
