using EventManager.Domain.Models;
using EventManager.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.DataAccess;

/// <summary>
/// Репозиторий для управления бронированиями событий, обеспечивающий операции создания, получения и удаления бронирований.
/// </summary>
/// <param name="context">Контекст базы данных.</param>
public class BookingRepository(AppDbContext context) : IBookingRepository
{
    private readonly AppDbContext _context = context;

    /// <inheritdoc/>
    public async Task<Booking> CreateAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        Booking newBooking = new()
        {
            Id = Guid.NewGuid(), // Создаем новое Id для брони.
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now.ToUniversalTime()
        };

        // Сохраняем изменения в базе данных.
        await _context.Bookings.AddAsync(newBooking, ct);
        await _context.SaveChangesAsync(ct);
        return newBooking;
    }
    
    /// <inheritdoc/>
    public async Task DeleteByIdAsync(Guid id, CancellationToken ct = default)
    {
        var existingBooking = await GetByIdAsync(id, ct);
        _context.Bookings.Remove(existingBooking);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public IQueryable<Booking> GetAll()
    {
        return _context.Bookings;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Booking>> GetBookingsByEventIdAsync(Guid eventId, CancellationToken ct = default)
    {
        var existingEvent = await _context.Events.FindAsync([ eventId ], cancellationToken: ct) ??
            throw new KeyNotFoundException($"Событие с Id:{eventId} не найдено.");
        return await _context.Bookings.Where(b => b.EventId == eventId).ToListAsync(ct);
        
    }

    /// <inheritdoc/>
    public async Task<Booking> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Bookings.FindAsync([ id ], cancellationToken: ct) ??
            throw new KeyNotFoundException($"Бронирование с Id:{id} не найдено.");
    }

    /// <inheritdoc/>
    public async Task ConfirmByIdAsync(Guid id, CancellationToken ct = default)
    {
        var existingBooking = await GetByIdAsync(id, ct);
        existingBooking.Confirm();
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task RejectByIdAsync(Guid id, CancellationToken ct = default)
    {
        var existingBooking = await GetByIdAsync(id, ct);
        existingBooking.Reject();
        await _context.SaveChangesAsync(ct);
    }

    public async Task CancelByIdAsync(Guid id, CancellationToken ct = default)
    {
        var existingBooking = await GetByIdAsync(id, ct);
        existingBooking.Cancel();
        await _context.SaveChangesAsync(ct);
    }
}
