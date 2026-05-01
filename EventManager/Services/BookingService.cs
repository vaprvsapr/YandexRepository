using EventManager.Data;
using EventManager.Interfaces;
using EventManager.Models.Bookings;
using EventManager.Models.Events;

namespace EventManager.Services;

/// <summary>
/// Сервис для управления бронированиями событий, реализующий бизнес-логику создания, получения и поиска бронирований.
/// </summary>
/// <param name="bookingRepository">Репозиторий для работы с бронированиями.</param>
/// <param name="eventRepository">Репозиторий для работы с событиями.</param>
public class BookingService
    (IRepository<Booking> bookingRepository, IRepository<Event> eventRepository) : IBookingService
{
    private readonly IRepository<Booking> _bookingRepository = bookingRepository;
    private readonly IRepository<Event> _eventRepository = eventRepository;

    // Пересмотреть урок, где было похожее. Нужно ли писать async, await?
    /// <inheritdoc/>
    public BookingDto CreateBookingAsync(Guid eventId)
    {
        // Проверка, что указанное событие существует.
        if (_eventRepository.GetById(eventId) is null)
            throw new KeyNotFoundException($"Событие с Id:{eventId} не найдено.");

        Booking newBooking = new()
        {
            Id = Guid.NewGuid(), // Создаем новое Id для брони.
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
            ProcessedAt = DateTime.Now
        };
        _bookingRepository.Add(newBooking);

        return BookingMapper.ToBookingDto(newBooking);
    }

    /// <inheritdoc/>
    public BookingDto? GetBookingByIdAsync(Guid id)
    {
        var existingBooking = _bookingRepository.GetById(id);
        return existingBooking is null
            ? throw new KeyNotFoundException($"Бронь с Id:{id} не найдена.")
            : BookingMapper.ToBookingDto(existingBooking);
    }

    /// <inheritdoc/>
    public List<BookingDto> GetBookingsByEventIdAsync(Guid eventId)
    {
        if(_eventRepository.GetById(eventId) is null)
            throw new KeyNotFoundException($"Событие с Id:{eventId} не найдено.");
        return _bookingRepository
            .GetAll()
            .Where(b => b.EventId == eventId)
            .Select(BookingMapper.ToBookingDto)
            .ToList();
    }
}
