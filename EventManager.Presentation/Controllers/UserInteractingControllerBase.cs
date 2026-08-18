using EventManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Presentation.Controllers;

/// <summary>
/// Базовый контроллер для взаимодействия с пользователем, 
/// предоставляющий методы для получения информации о пользователе из токена аутентификации.
/// </summary>
public abstract class UserInteractingControllerBase : ControllerBase
{
    /// <summary>
    /// Получает идентификатор пользователя из токена аутентификации (JWT) в виде Guid.
    /// </summary>
    protected Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub") ??
            throw new UnauthorizedAccessException("ID пользователя не найден в токене.");
        return Guid.Parse(userIdClaim.Value);
    }

    /// <summary>
    /// Получает роль пользователя из токена аутентификации (JWT) в виде перечисления UserRole.
    /// </summary>
    protected UserRole GetUserRoleFromClaims()
    {
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role");
        if (roleClaim == null)
            throw new InvalidOperationException("Роль пользователя не найдена в токене.");
        return Enum.Parse<UserRole>(roleClaim.Value);
    }

    /// <summary>
    /// Получает логин пользователя из токена аутентификации (JWT) в виде строки.
    /// </summary>
    protected string GetUserLoginFromClaims()
    {
        var loginClaim = User.Claims.FirstOrDefault(c => c.Type == "login");
        if (loginClaim == null)
            throw new InvalidOperationException("Логин пользователя не найден в токене.");
        return loginClaim.Value;
    }
}
