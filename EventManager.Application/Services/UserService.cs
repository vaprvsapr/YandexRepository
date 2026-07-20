using EventManager.Application.Repositories;
using EventManager.Application.Dto;
using EventManager.Application.Mappers;
using EventManager.Domain.Models;
using EventManager.Domain.Exceptions;
using EventManager.Application.Security;
using EventManager.Application.Services.Interfaces;

namespace EventManager.Application.Services;

/// <summary>
/// Предоставляет методы для управления пользователями, включая регистрацию, вход в систему и удаление пользователей.
/// </summary>
/// <param name="userRepository"></param>
/// <param name="tokenGeneratingService"></param>
public class UserService(IUserRepository userRepository, ITokenGeneratingService tokenGeneratingService) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITokenGeneratingService _tokenGeneratingService = tokenGeneratingService;

    /// <inheritdoc/>
    public async Task<UserInfoDto> Register(string login, string password, UserRole role)
    {
        var existingUser = await _userRepository.GetByLoginAsync(login);
        if (existingUser != null) 
            throw new InvalidOperationException($"Пользователь с логином {login} уже существует.");

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Role = role,
            Login = login,
            PasswordHash = PasswordManager.HashPassword(password)
        };

        await _userRepository.CreateAsync(newUser);
        return UserMapper.ToUserInfoDto(newUser);
    }

    /// <inheritdoc/>
    public async Task<string> LogIn(string login, string password)
    {
        var passwordHash = PasswordManager.HashPassword(password);
        var existingUser = await _userRepository.GetByLoginAsync(login) ??
            throw new KeyNotFoundException($"Не удалось войти в систему.");
        if (existingUser.PasswordHash == passwordHash)
            return _tokenGeneratingService.GenerateToken(existingUser.Id, existingUser.Login, existingUser.Role);
        throw new KeyNotFoundException($"Не удалось войти в систему.");
    }

    /// <inheritdoc/>
    public async Task Delete(string login)
    {
        var existingUser = await _userRepository.GetByLoginAsync(login) ?? 
            throw new KeyNotFoundException($"Пользователь с логином {login} не найден.");
        await _userRepository.DeleteAsync(existingUser);
    }
}
