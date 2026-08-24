using BookingMicroService.Domain;
using Common.Contracts;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;

namespace BookingMicroService.Application;

/// <summary>
/// Сервис для управления бронированиями событий, реализующий бизнес-логику создания, получения и поиска бронирований.
/// </summary>
public class BookingService(
    IBookingRepository bookingRepository,
    IBookingProcessRequestProducer bookingCreatedProducer,
    ILogger<BookingService> logger) : IBookingService
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IBookingProcessRequestProducer _bookingCreatedProducer = bookingCreatedProducer;
    private readonly ILogger<BookingService> _logger = logger;

    /// <inheritdoc/>
    public async Task<BookingDto> CreateAsync(Guid eventId, Guid userId)
    {
        var newBooking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _bookingRepository.CreateAsync(newBooking);

        var bookingProcessRequest = new BookingProcessRequest
        {
            BookingId = newBooking.Id,
            EventId = newBooking.EventId,
            UserId = newBooking.UserId,
            Command = BookingProcessCommand.Confirm
        };
        await _bookingCreatedProducer.PublishAsync(bookingProcessRequest);
        return BookingMapper.ToBookingDto(newBooking);
    }

    /// <inheritdoc/>
    public async Task CancelByIdAsync(Guid bookingId, Guid userId)
    {
        var existingBooking = await GetBookingByIdAsync(bookingId) ??
            throw new KeyNotFoundException($"Бронирование с Id:{bookingId} не найдено.");
        if (existingBooking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException($"Бронирование с Id:{bookingId} уже отменено.");
        if (existingBooking.UserId != userId)
            throw new AuthenticationException($"Пользователь с Id:{userId} не имеет прав на отмену бронирования с Id:{bookingId}.");

        var eventId = existingBooking.EventId;

        var bookingProcessRequest = new BookingProcessRequest
        {
            BookingId = existingBooking.Id,
            EventId = eventId,
            UserId = userId,
            Command = BookingProcessCommand.Cancel
        };

        await _bookingCreatedProducer.PublishAsync(bookingProcessRequest);
    }

    /// <inheritdoc/>   
    public async Task<BookingDto?> GetByIdAsync(Guid id)
    {
        return BookingMapper.ToBookingDto(await GetBookingByIdAsync(id));
    }

    /// <inheritdoc/>
    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        return [.. _bookingRepository
            .GetAll()
            .Select(BookingMapper.ToBookingDto)
            ];
    }

    private async Task<Booking> GetBookingByIdAsync(Guid id)
    {
        var existingBooking = await _bookingRepository.GetByIdAsync(id) ??
            throw new KeyNotFoundException($"Бронирование с Id:{id} не найдено.");
        return existingBooking;
    }
}
