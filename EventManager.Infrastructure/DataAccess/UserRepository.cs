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
    public async Task CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id); ;
    }

    /// <inheritdoc/>
    public async Task<User?> GetByLoginAsync(string login)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
    }
}
