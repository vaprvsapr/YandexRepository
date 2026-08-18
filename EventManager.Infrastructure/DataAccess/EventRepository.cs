using EventManager.Domain.Models;
using EventManager.Application.Repositories;
using EventManager.Application.Dto;

namespace EventManager.Infrastructure.DataAccess;

/// <summary>
/// Репозиторий для управления событиями, обеспечивающий операции создания, получения, обновления и удаления событий.
/// </summary>
/// <param name="context"></param>
public class EventRepository(AppDbContext context) : IEventRepository
{
    private readonly AppDbContext _context = context;

    /// <inheritdoc/>
    public async Task CreateAsync(Event @event, CancellationToken ct = default)
    {
        _context.Events.Add(@event);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Event @event, CancellationToken ct = default)
    {
        _context.Events.Remove(@event);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public IQueryable<Event> GetAll()
    {
        return _context.Events;
    }

    /// <inheritdoc/>
    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Events.FindAsync([ id ], cancellationToken: ct);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Event @event, CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
