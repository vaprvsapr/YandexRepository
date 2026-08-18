using UserMicroService.Domain;
using UserMicroService.Application;
using Microsoft.EntityFrameworkCore;

namespace UserMicroService.Infrastructure;

/// <summary>
/// Реализация репозитория пользователей, предоставляющая методы для управления данными пользователей в базе данных.
/// </summary>
/// <param name="context"></param>
public class UserRepository(UserDbContext context) : IUserRepository
{
    private readonly UserDbContext _context = context;

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
