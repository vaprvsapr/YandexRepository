using EventManager.Application.Dto;
using EventManager.Domain.Models;

namespace EventManager.Application.Repositories;

/// <summary>
/// Интерфейс репозитория событий, определяющий методы для управления данными событий в базе данных.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Асинхронно получает событие по его уникальному идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Получает все события в виде IQueryable, что позволяет выполнять дополнительные операции LINQ на уровне базы данных.
    /// </summary>
    public IQueryable<Event> GetAll();

    /// <summary>
    /// Асинхронно удаляет событие.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="ct"></param>
    public Task DeleteAsync(Event @event, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно обновляет существующее событие.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="ct"></param>
    public Task UpdateAsync(Event @event, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно создает новое событие на основе предоставленных данных и сохраняет его в базе данных.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="ct"></param>
    public Task CreateAsync(Event @event, CancellationToken ct = default);
}
