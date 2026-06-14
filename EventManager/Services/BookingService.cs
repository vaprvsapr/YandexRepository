using EventManager.DataAccess;
using EventManager.DataAccess.Repositories;
using EventManager.ExceptionHandling;
using EventManager.Interfaces;
using EventManager.Models.Bookings;
using EventManager.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Services
{
    /// <summary>
    /// Сервис для управления бронированиями событий, реализующий бизнес-логику создания, получения и поиска бронирований.
    /// </summary>
    /// <param name="bookingRepository">Репозиторий для управления бронированиями.</param>
    /// <param name="logger">Логгер для записи информации о процессе управления бронированиями.</param>
    public class BookingService(
        IBookingRepository bookingRepository,
        ILogger<BookingService> logger) : IBookingService
    {
        private readonly IBookingRepository _bookingRepository = bookingRepository;
        private readonly ILogger<BookingService> _logger = logger;

        /// <inheritdoc/>
        public async Task<BookingDto> CreateBookingAsync(Guid eventId)
        {
            Booking newBooking = await _bookingRepository.CreateAsync(eventId);

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
            await _bookingRepository.DeleteByIdAsync(id);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Deleted booking with Id:{id}.", id);
        }
    }
}
