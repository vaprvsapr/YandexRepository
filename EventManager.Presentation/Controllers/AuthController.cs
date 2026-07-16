
using EventManager.Application.Dto;
using EventManager.Application.Services;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

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

    [HttpGet]
    [Route("login")]
    public async Task<ActionResult<string>> LogIn(
        [FromQuery] string login, 
        [FromQuery] string password)
    {
        var token = await _userService.LogIn(login, password);
        return Ok(token);
    }

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