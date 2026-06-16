using EventManager.Models.Bookings;
namespace EventManager.DataAccess.Repositories;

/// <summary>
/// Интерфейс репозитория бронирований, определяющий методы для управления данными бронирований в базе данных.
/// </summary>
public interface IBookingRepository
{

    /// <summary>
    /// Метод получения бронирования по индентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<Booking?> GetByIdAsync(Guid id);

    /// <summary>
    /// Метод получения всех бронирований, связанных с определенным событием, по идентификатору события.
    /// </summary>
    /// <param name="eventId"></param>
    /// <returns></returns>
    public Task<IEnumerable<Booking>> GetBookingsByEventIdAsync(Guid eventId);

    /// <summary>
    /// Метод получения всех бронирований, доступных в базе данных.
    /// </summary>
    /// <returns></returns>
    public IQueryable<Booking> GetAll();

    /// <summary>
    /// Метод удаления бронирования по идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task DeleteByIdAsync(Guid id);

    /// <summary>
    /// Метод создания нового бронирования для определенного события по идентификатору события.
    /// </summary>
    /// <param name="eventId"></param>
    /// <returns></returns>
    public Task<Booking> CreateAsync(Guid eventId);

    public Task ConfirmByIdAsync(Guid id);

    public Task RejectByIdAsync(Guid id);
}
