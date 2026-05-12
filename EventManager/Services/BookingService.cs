using EventManager.Data;
using EventManager.Interfaces;
using EventManager.Models.Bookings;
using EventManager.Models.Events;
using EventManager.ExceptionHandling;

namespace EventManager.Services;

/// <summary>
/// Сервис для управления бронированиями событий, реализующий бизнес-логику создания, получения и поиска бронирований.
/// </summary>
/// <param name="bookingRepository">Репозиторий для работы с бронированиями.</param>
/// <param name="eventRepository">Репозиторий для работы с событиями.</param>
/// <param name="logger">Логгер для записи информации о процессе управления бронированиями.</param>
public class BookingService
    (IRepository<Booking> bookingRepository, IRepository<Event> eventRepository, ILogger<BookingService> logger) : IBookingService
{
    private readonly IRepository<Booking> _bookingRepository = bookingRepository;
    private readonly IRepository<Event> _eventRepository = eventRepository;
    private readonly ILogger<BookingService> _logger = logger;
    private readonly Lock _bookingLock = new();

    /// <inheritdoc/>
    public async Task<BookingDto>CreateBookingAsync(Guid eventId)
    {
        lock (_bookingLock)
        {
            // Проверка, что указанное событие существует.
            Event? existingEvent = _eventRepository.GetById(eventId) ?? 
                throw new KeyNotFoundException($"Событие с Id:{eventId} не найдено.");

            if (!existingEvent.TryReserveSeats())
                throw new NoAvailableSeatsException($"Нет доступных мест для события с Id:{eventId}.");

            Booking newBooking = new()
            {
                Id = Guid.NewGuid(), // Создаем новое Id для брони.
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now
            };

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Created new booking with Id:{id} for EventId:{eventId}.", newBooking.Id, newBooking.EventId);
            _bookingRepository.Add(newBooking);
            return BookingMapper.ToBookingDto(newBooking);
        }
    }

    /// <inheritdoc/>
    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        return [.. _bookingRepository.GetAll().Select(BookingMapper.ToBookingDto)];
    }

    /// <inheritdoc/>
    public async Task<BookingDto?> GetBookingByIdAsync(Guid id)
    {
        var existingBooking = _bookingRepository.GetById(id);
        return existingBooking is null
            ? throw new KeyNotFoundException($"Бронь с Id:{id} не найдена.")
            : BookingMapper.ToBookingDto(existingBooking);
    }

    /// <inheritdoc/>
    public async Task<List<BookingDto>> GetBookingsByEventIdAsync(Guid eventId)
    {
        if(_eventRepository.GetById(eventId) is null)
            throw new KeyNotFoundException($"Событие с Id:{eventId} не найдено.");
        return [.. _bookingRepository
            .GetAll()
            .Where(b => b.EventId == eventId)
            .Select(BookingMapper.ToBookingDto)];
    }


}
