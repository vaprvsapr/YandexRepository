using EventManager.Domain.Models;
using EventManager.Domain.Exceptions;
using EventManager.Application.Dto;
using EventManager.Application.Repositories;
using EventManager.Application.Mappers;
using Microsoft.Extensions.Logging;
using EventManager.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace EventManager.Application.Services;

/// <summary>
/// Сервис для управления бронированиями событий, реализующий бизнес-логику создания, получения и поиска бронирований.
/// </summary>
/// <param name="bookingRepository">Репозиторий бронирований.</param>
/// <param name="eventRepository">Репозиторий событий.</param>
/// <param name="userRepository">Репозиторий пользователей.</param>
/// <param name="logger">Логгер для записи информации о процессе управления бронированиями.</param>
public class BookingService(
    IBookingRepository bookingRepository,
    IEventRepository eventRepository,
    IUserRepository userRepository,
    ILogger<BookingService> logger,
    IConfiguration configuration) : IBookingService
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILogger<BookingService> _logger = logger;
    private readonly int _maxActiveBookingsPerUser = int.Parse(configuration["User:MaxActiveBookings"] ?? "1"); // Максимальное количество активных бронирований на пользователя
    private readonly SemaphoreSlim _bookingSemaphore = new(1, 1); // Семафор для синхронизации доступа к бронированию мест

    /// <inheritdoc/>
    public async Task<BookingDto> CreateBookingAsync(Guid eventId, Guid userId)
    {
        Event existingEvent = await _eventRepository.GetByIdAsync(eventId);
        User existingUser = await _userRepository.GetByIdAsync(userId);

        Booking newBooking;

        await _bookingSemaphore.WaitAsync();
        try
        {
            if (existingEvent.StartAt <= DateTime.UtcNow)
                throw new PastEventBookingException($"Невозможно создать бронирование для события с id: {eventId}, так как оно уже началось или завершилось.");

            var activeBookingsCount = _bookingRepository.GetAll()
                .Where(b => b.UserId == userId && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
                .Count(b => b.Event.StartAt > DateTime.UtcNow);
            if (activeBookingsCount >= _maxActiveBookingsPerUser)
                throw new ExceedingActiveBookingLimitException($"Пользователь с id: {userId} превысил лимит активных бронирований. Лимит: {_maxActiveBookingsPerUser}");

            if (!existingEvent.TryReserveSeats())
                throw new NoAvailableSeatsException($"Нет достаточного количества свободных мест на событие с id: {eventId}.");

            await _eventRepository.UpdateAsync(existingEvent);
            newBooking = await _bookingRepository.CreateAsync(eventId, userId);
        }
        catch (NoAvailableSeatsException)
        {
            throw;
        }
        finally
        {
            _bookingSemaphore.Release();
        }

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Created new booking with Id:{id} for EventId:{eventId}.", 
                newBooking.Id, newBooking.EventId);

        return BookingMapper.ToBookingDto(newBooking);
    }

    /// <inheritdoc/>
    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        return [.. _bookingRepository
            .GetAll()
            .Select(BookingMapper.ToBookingDto)
            ];
    }

    /// <inheritdoc/>   
    public async Task<BookingDto?> GetBookingByIdAsync(Guid id)
    {
        var existingBooking = await _bookingRepository.GetByIdAsync(id);
        return BookingMapper.ToBookingDto(existingBooking);
    }

    /// <inheritdoc/>
    public async Task<List<BookingDto>> GetBookingsByEventIdAsync(Guid eventId)
    {
        var bookings = await _bookingRepository.GetBookingsByEventIdAsync(eventId);
        return [.. bookings.Select(BookingMapper.ToBookingDto)];
    }

    /// <inheritdoc/>
    public async Task DeleteBookingByIdAsync(Guid id)
    {
        var existingBooking = await _bookingRepository.GetByIdAsync(id);
        var existingEvent = await _eventRepository.GetByIdAsync(existingBooking.EventId);

        await _bookingRepository.DeleteByIdAsync(id);

        if(existingBooking.Status != BookingStatus.Cancelled && existingBooking.Status != BookingStatus.Rejected)
        {
            existingEvent.ReleaseSeats();
            await _eventRepository.UpdateAsync(existingEvent);
        }

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Deleted booking with Id:{id}.", id);
    }

    public async Task<BookingDto> CancelBookingByIdAsync(Guid id)
    {
        var existingBooking = await _bookingRepository.GetByIdAsync(id);
        if (existingBooking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException($"Booking with Id:{id} is already cancelled.");
        var existingEvent = await _eventRepository.GetByIdAsync(existingBooking.EventId);

        await _bookingRepository.CancelByIdAsync(id);

        existingEvent.ReleaseSeats();
        await _eventRepository.UpdateAsync(existingEvent);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Cancelled booking with Id:{id}.", id);

        return BookingMapper.ToBookingDto(existingBooking);
    }
}
