using EventManager.Domain.Models;

namespace EventManager.Application.Repositories;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User> GetByIdAsync(Guid id);

    Task<User> GetByLoginAsync(string login);

    Task<IReadOnlyList<User>> GetAllAsync();

    Task<User> UpdateAsync(User user);

    Task DeleteAsync(Guid id);
}
