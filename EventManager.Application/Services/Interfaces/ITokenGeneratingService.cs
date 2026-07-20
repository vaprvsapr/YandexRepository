using EventManager.Domain.Models;

namespace EventManager.Application.Services.Interfaces;

/// <summary>
/// Определяет контракт сервиса для генерации токенов аутентификации пользователей.
/// </summary>
public interface ITokenGeneratingService
{
    /// <summary>
    /// Генерирует токен аутентификации для пользователя на основе его идентификатора, логина и роли.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="login"></param>
    /// <param name="role"></param>
    /// <returns>Токен аутентификации.</returns>
    string GenerateToken(Guid userId, string login, UserRole role);
}
