using EventManager.Models.Events;
namespace EventManager.DataAccess.Repositories;


/// <summary>
/// Интерфейс репозитория событий, определяющий методы для управления данными событий в базе данных.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Асинхронно получает событие по его уникальному идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<Event> GetByIdAsync(Guid id);

    /// <summary>
    /// Получает все события в виде IQueryable, что позволяет выполнять дополнительные операции LINQ на уровне базы данных.
    /// </summary>
    /// <returns></returns>
    public IQueryable<Event> GetAll();

    /// <summary>
    /// Асинхронно удаляет событие по его уникальному идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task DeleteByIdAsync(Guid id);

    /// <summary>
    /// Асинхронно обновляет существующее событие.
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    public Task<Event> UpdateAsync(Event @event);

    /// <summary>
    /// Асинхронно создает новое событие на основе предоставленных данных и сохраняет его в базе данных.
    /// </summary>
    /// <param name="eventCreateDto"></param>
    /// <returns></returns>
    public Task<Event> CreateAsync(EventCreateDto eventCreateDto);
}
