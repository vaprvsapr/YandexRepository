
using EventManager.Application.Dto;
using EventManager.Application.Services;
using EventManager.Domain.Models;
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
}