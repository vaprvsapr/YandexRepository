using EventManager.Application.Dto;
using EventManager.Domain.Models;

namespace EventManager.Application.Services;
public interface IUserService
{
    Task<UserInfoDto> Register(string login, string password, UserRole role);

    Task<string> LogIn(string login, string password);
}
