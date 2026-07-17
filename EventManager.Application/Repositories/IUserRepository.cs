using EventManager.Domain.Models;

namespace EventManager.Application.Repositories;

/// <summary>
/// Интерфейс репозитория для управления пользователями, предоставляющий методы для создания, получения, обновления и удаления пользователей в системе.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Создает нового пользователя в системе.
    /// </summary>
    /// <param name="user">Пользователь для создания.</param>
    /// <returns>Созданный пользователь.</returns>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Получает пользователя по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя.</param>
    /// <returns>Пользователь с указанным идентификатором.</returns>
    Task<User> GetByIdAsync(Guid id);

    /// <summary>
    /// Получает пользователя по его логину.
    /// </summary>
    /// <param name="login">Логин пользователя.</param>
    /// <returns>Пользователь с указанным логином.</returns>
    Task<User> GetByLoginAsync(string login);

    /// <summary>
    /// Получает список всех пользователей.
    /// </summary>
    /// <returns>Список пользователей.</returns>
    Task<IReadOnlyList<User>> GetAllAsync();

    /// <summary>
    /// Обновляет информацию о пользователе.
    /// </summary>
    /// <param name="user">Пользователь для обновления.</param>
    /// <returns>Обновленный пользователь.</returns>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Удаляет пользователя по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя.</param>
    Task DeleteAsync(Guid id);
}
