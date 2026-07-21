using EventManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Presentation.Controllers;

public class UserInteractingControllerBase : ControllerBase
{
    protected Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
        return userId;
    }

    protected UserRole GetUserRoleFromClaims()
    {
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role");
        if (roleClaim == null)
            throw new InvalidOperationException("Роль пользователя не найдена в токене.");
        return Enum.Parse<UserRole>(roleClaim.Value);
    }

    protected string GetUserLoginFromClaims()
    {
        var loginClaim = User.Claims.FirstOrDefault(c => c.Type == "login");
        if (loginClaim == null)
            throw new InvalidOperationException("Логин пользователя не найден в токене.");
        return loginClaim.Value;
    }
}
