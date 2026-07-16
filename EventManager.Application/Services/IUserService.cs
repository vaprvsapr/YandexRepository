using EventManager.Application.Dto;
using EventManager.Domain.Models;
using System.Security.Cryptography.X509Certificates;

namespace EventManager.Application.Services;
public interface IUserService
{
    Task<UserInfoDto> Register(string login, string password, UserRole role);

    Task<string> LogIn(string login, string password);

    Task Delete(string login);
}
