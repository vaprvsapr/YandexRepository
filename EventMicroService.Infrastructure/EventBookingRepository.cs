using EventMicroService.Domain;
using EventMicroService.Application;
using Microsoft.EntityFrameworkCore;

namespace EventMicroService.Infrastructure;

public class EventBookingRepository(EventDbContext context) : IEventBookingRepository
{
    private readonly EventDbContext _context = context;

    /// <inheritdoc/>
    public async Task CreateAsync(EventBooking eventBooking, CancellationToken ct = default)
    {
        _context.EventBookings.Add(eventBooking);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(EventBooking eventBooking, CancellationToken ct = default)
    {
        _context.EventBookings.Remove(eventBooking);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<EventBooking?> GetEventBookingsAsync(Guid eventId, Guid bookingId, CancellationToken ct = default)
    {
        return await _context.EventBookings
            .Where(eb => eb.EventId == eventId)
            .Where(eb => eb.BookingId == bookingId)
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<EventBooking?> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await _context.EventBookings.FindAsync([bookingId], cancellationToken: ct);
    }
}
