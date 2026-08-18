using EventManager.Domain.Models;

namespace EventManager.Application.Repositories;

/// <summary>
/// Интерфейс репозитория бронирований, определяющий методы для управления данными бронирований в базе данных.
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Метод создания нового бронирования для определенного события по идентификатору события.
    /// </summary>
    /// <param name="booking"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task CreateAsync(Booking booking, CancellationToken ct = default);

    /// <summary>
    /// Метод получения бронирования по индентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Метод получения всех бронирований, доступных в базе данных.
    /// </summary>
    /// <returns></returns>
    public IQueryable<Booking> GetAll();

    /// <summary>
    /// Метод удаления бронирования по идентификатору.
    /// </summary>
    /// <param name="booking"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task DeleteAsync(Booking booking, CancellationToken ct = default);

    /// <summary>
    /// Подтверждает бронирование по его уникальному идентификатору, 
    /// изменяя его статус на "Подтверждено" и сохраняет изменения в базе данных.
    /// </summary>
    /// <param name="booking"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task ConfirmAsync(Booking booking, CancellationToken ct = default);

    /// <summary>
    /// Отклонеяет бронирование по его уникальному идентификатору,
    /// изменяя его статус на "Отклонено" и сохраняет изменения в базе данных.
    /// </summary>
    /// <param name="booking"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task RejectAsync(Booking booking, CancellationToken ct = default);

    /// <summary>
    /// Отменяет бронирование по его уникальному идентификатору,
    /// изменяя его статус на "Отменено" и скохраняет изменения в базе данных.
    /// </summary>
    /// <param name="booking"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task CancelAsync(Booking booking, CancellationToken ct = default);
}
