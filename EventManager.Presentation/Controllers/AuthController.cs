
using EventManager.Application.Dto;
using EventManager.Application.Services.Interfaces;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Presentation.Controllers;

/// <summary>
/// Контроллер для управления аутентификацией и регистрацией пользователей, предоставляющий методы для регистрации, входа в систему и удаления пользователей.
/// </summary>
/// <param name="userService"></param>
[ApiController]
[Route("[controller]")]
public class AuthController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    /// <summary>
    /// Регистрация нового пользователя с указанным логином, паролем и ролью.
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <param name="role"></param>
    /// <returns>Возвращает информацию о зарегистрированном пользователе.</returns>
    [HttpPost]
    [Route("register")]
    public async Task<ActionResult<UserInfoDto>> Register(
        [FromQuery] string login, 
        [FromQuery] string password, 
        [FromQuery] UserRole role = UserRole.User)
    {
        var userInfo = await _userService.Register(login, password, role);
        return Ok(userInfo);
    }

    /// <summary>
    /// Вход пользователя в систему с указанным логином и паролем, возвращает токен аутентификации.
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <returns>Возвращает токен аутентификации.</returns>
    [HttpGet]
    [Route("login")]
    public async Task<ActionResult<string>> LogIn(
        [FromQuery] string login, 
        [FromQuery] string password)
    {
        var token = await _userService.LogIn(login, password);
        return Ok(token);
    }

    /// <summary>
    /// Удаление пользователя с указанным логином. Доступно только для администраторов или самого пользователя.
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    [Authorize]
    [HttpDelete]
    [Route("delete")]
    public async Task<ActionResult> DeleteUser([FromQuery] string login)
    {
        if(!GetUserRoleFromClaims().Equals(UserRole.Admin) && !GetUserLoginFromClaims().Equals(login))
            return Forbid("Недостаточно прав для удаления пользователя.");

        await _userService.Delete(login);
        return NoContent();
    }

    private UserRole GetUserRoleFromClaims()
    {
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role");
        if (roleClaim == null)
            throw new InvalidOperationException("Роль пользователя не найдена в токене.");
        return Enum.Parse<UserRole>(roleClaim.Value);
    }

    private string GetUserLoginFromClaims()
    {
        var loginClaim = User.Claims.FirstOrDefault(c => c.Type == "login");
        if (loginClaim == null)
            throw new InvalidOperationException("Логин пользователя не найден в токене.");
        return loginClaim.Value;
    }
}