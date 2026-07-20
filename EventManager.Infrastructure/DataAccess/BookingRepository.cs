using EventManager.Domain.Models;
using EventManager.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.DataAccess;

/// <summary>
/// Репозиторий для управления бронированиями событий, 
/// обеспечивающий операции создания, получения и удаления бронирований.
/// </summary>
/// <param name="context">Контекст базы данных.</param>
public class BookingRepository(AppDbContext context) : IBookingRepository
{
    private readonly AppDbContext _context = context;

    /// <inheritdoc/>
    public async Task CreateAsync(Booking booking, CancellationToken ct = default)
    {
        await _context.Bookings.AddAsync(booking, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public IQueryable<Booking> GetAll()
    {
        return _context.Bookings;
    }

    /// <inheritdoc/>
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Bookings.FindAsync([ id ], cancellationToken: ct);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Booking booking, CancellationToken ct = default)
    {
        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task ConfirmAsync(Booking booking, CancellationToken ct = default)
    {
        booking.Confirm();
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task RejectAsync(Booking booking, CancellationToken ct = default)
    {
        booking.Reject();
        await _context.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(Booking booking, CancellationToken ct = default)
    {
        booking.Cancel();
        await _context.SaveChangesAsync(ct);
    }
}
