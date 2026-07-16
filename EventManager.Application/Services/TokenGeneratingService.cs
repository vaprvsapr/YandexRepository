using EventManager.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventManager.Application.Services;

public class TokenGeneratingService
{
    private readonly IConfiguration _configuration;

    public TokenGeneratingService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Guid userId, string login, UserRole role)
    {
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = userId.ToString(),
            ["login"] = login,
            ["role"] = role.ToString()
        };

        var jwtKey = _configuration["JWT:SekretKey"] ?? throw new InvalidOperationException("JWT:SekretKey is missing");
        var issuer = _configuration["JWT:Issuer"];
        var audience = _configuration["JWT:Audience"];
        var lifetime = TimeSpan.Parse(_configuration["JWT:TokenLifetime"] ?? "00:15:00");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Claims = claims,
            Expires = DateTime.UtcNow.Add(lifetime),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = creds
        };

        var tokenString = new JsonWebTokenHandler().CreateToken(descriptor);

        return tokenString;
    }
}
