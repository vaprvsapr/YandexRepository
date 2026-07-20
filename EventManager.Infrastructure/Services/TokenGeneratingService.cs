using EventManager.Domain.Models;
using EventManager.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventManager.Infrastructure.Services;

/// <summary>
/// Сервис для генерации JWT токенов для аутентификации пользователей.
/// </summary>
/// <param name="configuration">Конфигурация приложения для получения настроек JWT.</param>
public class TokenGeneratingService(IConfiguration configuration) : ITokenGeneratingService
{
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Генерирует JWT токен для указанного пользователя с заданными идентификатором, логином и ролью.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="login">Логин пользователя.</param>
    /// <param name="role">Роль пользователя.</param>
    /// <returns>JWT токен.</returns>
    /// <exception cref="SecurityTokenEncryptionKeyNotFoundException"></exception>
    public string GenerateToken(Guid userId, string login, UserRole role)
    {
        var claims = new Dictionary<string, object>
        {
            ["sub"] = userId.ToString(),
            ["login"] = login,
            ["role"] = role.ToString()
        };

        var jwtKey = _configuration["JWT:SekretKey"] ?? 
            throw new SecurityTokenEncryptionKeyNotFoundException("JWT:SekretKey is missing");
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
