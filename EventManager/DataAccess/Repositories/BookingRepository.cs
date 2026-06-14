using EventManager.ExceptionHandling;
using EventManager.Models.Bookings;
using EventManager.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace EventManager.DataAccess.Repositories;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    private readonly AppDbContext _context = context;
    private readonly Lock _bookingLock = new();
    public async Task<Booking> CreateAsync(Guid eventId)
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

        // Сохраняем изменения в базе данных.
        await _context.Bookings.AddAsync(newBooking);
        await _context.SaveChangesAsync();
        return newBooking;
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        var existingBooking = _context.Bookings.Find(id) ??
            throw new KeyNotFoundException($"Бронь с Id:{id} не найдена.");
        _context.Bookings.Remove(existingBooking);
        await _context.SaveChangesAsync();
    }

    public IQueryable<Booking> GetAll()
    {
        return _context.Bookings;
    }

    public async Task<IEnumerable<Booking>> GetBookingsByEventIdAsync(Guid eventId)
    {
        var existingEvent = await _context.Events.FindAsync(eventId) ??
            throw new KeyNotFoundException($"Событие с Id:{eventId} не найдено.");
        return await _context.Bookings.Where(b => b.EventId == eventId).ToListAsync();
        
    }

    public async Task<Booking> GetByIdAsync(Guid id)
    {
        var existingBooking = await _context.Bookings.FindAsync(id) ??
                throw new KeyNotFoundException("Бронь с Id:{id} не найдена.");
        return existingBooking;
    }
}
