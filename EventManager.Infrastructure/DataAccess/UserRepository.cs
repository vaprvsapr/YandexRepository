using EventManager.Domain.Models;
using EventManager.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.DataAccess;

/// <summary>
/// Реализация репозитория пользователей, предоставляющая методы для управления данными пользователей в базе данных.
/// </summary>
/// <param name="context"></param>
public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    /// <inheritdoc/>
    public async Task<User> CreateAsync(User user)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existingUser != null)
            throw new InvalidOperationException($"User with ID {user.Id} already exists.");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        var createdUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id) ?? 
            throw new KeyNotFoundException($"Не удалось создать пользователя с ID {user.Id}");

        return createdUser;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        var existingUser = await GetByIdAsync(id);
        _context.Users.Remove(existingUser);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<User> GetByIdAsync(Guid id)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id) ??
            throw new KeyNotFoundException($"Пользователь с ID {id} не найден.");
        return existingUser;
    }

    /// <inheritdoc/>
    public async Task<User> GetByLoginAsync(string login)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == login) ??
            throw new KeyNotFoundException($"Пользователь с логином {login} не найден.");
        return existingUser;
    }

    /// <inheritdoc/>
    public async Task<User> UpdateAsync(User user)
    {
        var existingUser = await GetByIdAsync(user.Id);

        existingUser.Role = user.Role;
        existingUser.Login = user.Login;
        existingUser.PasswordHash = user.PasswordHash;

        await _context.SaveChangesAsync();

        var updatedUser = await GetByIdAsync(user.Id) ??
            throw new KeyNotFoundException($"Не удалось обновить пользователя с ID {user.Id}");
        return updatedUser;
    }
}
