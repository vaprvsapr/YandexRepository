using EventManager.Application.Dto;

namespace EventManager.Application.Services.Interfaces;

/// <summary>
/// Определяет контракт сервиса для управления бронированиями событий.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Создаёт новое бронирование для указанного события.
    /// </summary>
    /// <param name="eventId">Идентификатор события, для которого создаётся бронирование.</param>
    /// <param name="userId">Идентификатор пользователя, который создаёт бронирование.</param>
    /// <returns>Данные созданного бронирования.</returns>
    public Task<BookingDto> CreateAsync(Guid eventId, Guid userId);

    /// <summary>
    /// Возвращает бронирование по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор бронирования.</param>
    /// <returns>Данные найденного бронирования или <see langword="null"/>, если не найдено.</returns>
    public Task<BookingDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Возвращает список всех бронирований.
    /// </summary>
    public Task<List<BookingDto>> GetAllBookingsAsync();

    public Task<BookingDto> CancelByIdAsync(Guid bookingId, UserInfoDto userInfoDto);
}
