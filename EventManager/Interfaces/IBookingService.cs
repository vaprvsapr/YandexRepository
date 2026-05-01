using EventManager.Models.Bookings;

namespace EventManager.Interfaces;

/// <summary>
/// Определяет контракт сервиса для управления бронированиями событий.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Создаёт новое бронирование для указанного события.
    /// </summary>
    /// <param name="eventId">Идентификатор события, для которого создаётся бронирование.</param>
    /// <returns>Данные созданного бронирования.</returns>
    public BookingDto CreateBookingAsync(Guid eventId);

    /// <summary>
    /// Возвращает бронирование по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор бронирования.</param>
    /// <returns>Данные найденного бронирования или <see langword="null"/>, если не найдено.</returns>
    public BookingDto? GetBookingByIdAsync(Guid id);

    /// <summary>
    /// Возвращает список бронирований для указанного события.
    /// </summary>
    /// <param name="eventId">Идентификатор события.</param>
    /// <returns>Список бронирований для события.</returns>
    public List<BookingDto> GetBookingsByEventIdAsync(Guid eventId);
}
