using EventManager.Domain.Models;
using EventManager.Domain.Exceptions;
using EventManager.Application.Dto;
using EventManager.Application.Repositories;
using EventManager.Application.Mappers;
using Microsoft.Extensions.Logging;

namespace EventManager.Application.Services;

/// <summary>
/// Сервис для управления бронированиями событий, реализующий бизнес-логику создания, получения и поиска бронирований.
/// </summary>
/// <param name="bookingRepository">Репозиторий бронирований.</param>
/// <param name="eventRepository">Репозиторий событий.</param>
/// <param name="logger">Логгер для записи информации о процессе управления бронированиями.</param>
public class BookingService(
    IBookingRepository bookingRepository,
    IEventRepository eventRepository,
    ILogger<BookingService> logger) : IBookingService
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly ILogger<BookingService> _logger = logger;
    private readonly SemaphoreSlim _bookingSemaphore = new(1, 1); // Семафор для синхронизации доступа к бронированию мест

    /// <inheritdoc/>
    public async Task<BookingDto> CreateBookingAsync(Guid eventId, Guid userId)
    {
        Event existingEvent = await _eventRepository.GetByIdAsync(eventId);
        Booking newBooking;

        await _bookingSemaphore.WaitAsync();
        try
        {
            if (existingEvent.TryReserveSeats())
            {
                await _eventRepository.UpdateAsync(existingEvent);
                newBooking = await _bookingRepository.CreateAsync(eventId, userId);
            }
            else
            {
                throw new NoAvailableSeatsException($"Нет достаточного количества свободных мест на событие с id: {eventId}.");
            }
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
        existingEvent.ReleaseSeats();
        await _eventRepository.UpdateAsync(existingEvent);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Deleted booking with Id:{id}.", id);
    }
}
