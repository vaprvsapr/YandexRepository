using EventManager.Application.Repositories;
using EventManager.Application.Dto;
using EventManager.Application.Mappers;
using EventManager.Domain.Models;
using EventManager.Application.Security;

namespace EventManager.Application.Services;

public class UserService(IUserRepository userRepository, TokenGeneratingService tokenGeneratingService) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly TokenGeneratingService _tokenGeneratingService = tokenGeneratingService;

    public async Task<UserInfoDto> Register(string login, string password, UserRole role)
    {
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Role = role,
            Login = login,
            PasswordHash = PasswordManager.HashPassword(password)
        };

        var createUser = await _userRepository.CreateAsync(newUser);

        return UserMapper.ToUserInfoDto(createUser);
    }

    public async Task<string> LogIn(string login, string password)
    {
        var passwordHash = PasswordManager.HashPassword(password);
        var existingUser = await _userRepository.GetByLoginAsync(login);
        if (existingUser.PasswordHash == passwordHash)
            return _tokenGeneratingService.GenerateToken(existingUser.Id, existingUser.Login, existingUser.Role);
        throw new InvalidOperationException($"Пароль для логина {login} не подходит.");
    }

    public async Task Delete(string login)
    {
        var existingUser = await _userRepository.GetByLoginAsync(login);
        if (existingUser == null)
            throw new InvalidOperationException($"Пользователь с логином {login} не найден.");
        await _userRepository.DeleteAsync(existingUser.Id);
    }
}
