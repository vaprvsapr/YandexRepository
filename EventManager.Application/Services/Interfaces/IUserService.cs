using EventManager.Application.Dto;
using EventManager.Domain.Models;

namespace EventManager.Application.Services.Interfaces;

/// <summary>
/// Предоставляет методы для управления пользователями, включая регистрацию, вход в систему и удаление пользователей.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Регистрирует нового пользователя с указанными учетными данными и ролью.
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    Task<UserInfoDto> Register(string login, string password, UserRole role);

    /// <summary>
    /// Выполняет вход пользователя в систему с указанными учетными данными и возвращает токен аутентификации.
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<string> LogIn(string login, string password);

    /// <summary>
    /// Удаляет пользователя с указанным логином из системы.
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    Task Delete(string login);
}
