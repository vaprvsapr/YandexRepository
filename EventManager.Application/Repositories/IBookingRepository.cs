using EventManager.Domain.Models;

namespace EventManager.Application.Repositories;

/// <summary>
/// Интерфейс репозитория бронирований, определяющий методы для управления данными бронирований в базе данных.
/// </summary>
public interface IBookingRepository
{

    /// <summary>
    /// Метод получения бронирования по индентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Booking> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Метод получения всех бронирований, связанных с определенным событием, по идентификатору события.
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<IEnumerable<Booking>> GetBookingsByEventIdAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    /// Метод получения всех бронирований, доступных в базе данных.
    /// </summary>
    /// <returns></returns>
    public IQueryable<Booking> GetAll();

    /// <summary>
    /// Метод удаления бронирования по идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Метод создания нового бронирования для определенного события по идентификатору события.
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Booking> CreateAsync(Guid eventId, Guid userId, CancellationToken ct = default);


    /// <summary>
    /// Подтверждает бронирование по его уникальному идентификатору, 
    /// изменяя его статус на "Подтверждено" и сохраняет изменения в базе данных.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task ConfirmByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Отклонеяет бронирование по его уникальному идентификатору,
    /// изменяя его статус на "Отклонено" и сохраняет изменения в базе данных.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task RejectByIdAsync(Guid id, CancellationToken ct = default);

    public Task CancelByIdAsync(Guid id, CancellationToken ct = default);
}
