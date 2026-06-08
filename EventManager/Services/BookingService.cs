using EventManager.DataAccess;
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
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="logger">Логгер для записи информации о процессе управления бронированиями.</param>
    public class BookingService(AppDbContext context, ILogger<BookingService> logger) : IBookingService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<BookingService> _logger = logger;
        private readonly Lock _bookingLock = new();

        /// <inheritdoc/>
        public async Task<BookingDto> CreateBookingAsync(Guid eventId)
        {
            // Проверка, что указанное событие существует.
            Event? existingEvent = await _context.Events.FindAsync(eventId) ??
                throw new KeyNotFoundException($"Событие с Id:{eventId} не найдено.");

            // Блокируем доступ к бронированию мест для данного события, чтобы избежать гонок при одновременных запросах.
            lock (_bookingLock)
            {
                if (!existingEvent.TryReserveSeats())
                    throw new NoAvailableSeatsException
                        ($"Нет доступных мест для события с Id:{eventId}.");
            }

            // Создаем новую бронь для указанного события.
            Booking newBooking = new()
            {
                Id = Guid.NewGuid(), // Создаем новое Id для брони.
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now.ToUniversalTime()
            };

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Created new booking with Id:{id} for EventId:{eventId}.", newBooking.Id, newBooking.EventId);

            // Сохраняем изменения в базе данных.
            await _context.Bookings.AddAsync(newBooking);
            await _context.SaveChangesAsync();
            return BookingMapper.ToBookingDto(newBooking);
        }

        /// <inheritdoc/>
        public async Task<List<BookingDto>> GetAllBookingsAsync()
        {
            var bookings = await _context.Bookings.ToListAsync();
            return [.. bookings.Select(BookingMapper.ToBookingDto)];
        }

        /// <inheritdoc/>   
        public async Task<BookingDto?> GetBookingByIdAsync(Guid id)
        { 
            var existingBooking = await _context.Bookings.FindAsync(id) ?? 
                throw new KeyNotFoundException("Бронь с Id:{id} не найдена.");
            return BookingMapper.ToBookingDto(existingBooking);
        }

        /// <inheritdoc/>
        public async Task<List<BookingDto>> GetBookingsByEventIdAsync(Guid eventId)
        {
            var existingEvent = await _context.Events.FindAsync(eventId) ??
                throw new KeyNotFoundException($"Событие с Id:{eventId} не найдено.");
            var bookings = await _context.Bookings.Where(b => b.EventId == eventId).ToListAsync();
            return [.. bookings.Select(BookingMapper.ToBookingDto)];
        }
    }
}
