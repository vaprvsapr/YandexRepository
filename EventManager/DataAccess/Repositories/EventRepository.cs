using EventManager.Models.Events;
using EventManager.Services;
using Microsoft.EntityFrameworkCore;

namespace EventManager.DataAccess.Repositories;

public class EventRepository(AppDbContext context) : IEventRepository
{
    private readonly AppDbContext _context = context;
    public async Task<Event> CreateAsync(EventCreateDto eventCreateDto)
    {
        var existingEvent = await _context.Events.FindAsync(eventCreateDto.Id);
        if (existingEvent is not null)
            throw new InvalidOperationException($"Событие с id {eventCreateDto.Id} уже существует.");
        var newEvent = EventMapper.ToEvent(eventCreateDto);
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return newEvent;
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        var existingEvent = await _context.Events.FindAsync(id) ??
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
        _context.Events.Remove(existingEvent);
        await _context.SaveChangesAsync();
    }

    public IQueryable<Event> GetAll()
    {
        return _context.Events;
    }

    public async Task<Event> GetByIdAsync(Guid id)
    {
        var existingEvent = await _context.Events.FindAsync(id) ??
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
        return existingEvent;
    }

    public async Task<Event> UpdateAsync(Event @event)
    {
        var existingEvent = await _context.Events.FindAsync(@event.Id) ??
            throw new KeyNotFoundException($"Событие с id {@event.Id} не найдено.");
        existingEvent.Title = @event.Title;
        existingEvent.Description = @event.Description;
        existingEvent.StartAt = @event.StartAt;
        existingEvent.EndAt = @event.EndAt;
        existingEvent.TotalSeats = @event.TotalSeats;
        await _context.SaveChangesAsync();
        return existingEvent;
    }
}
