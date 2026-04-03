using EventManager.Interfaces;
using EventManager.Models;

namespace EventManager.Data;

/// <inheritdoc/>
public class EventRepository : IEventRepository
{
    private readonly List<Event> _events = [];

    /// <inheritdoc />
    public bool Add(Event newEvent)
    {
        var existingEvent = _events.FirstOrDefault(e => e.Id == newEvent.Id);
        if (existingEvent == null)
        {
            _events.Add(newEvent);
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public bool Delete(int id)
    {
        var existingEvent = _events.FirstOrDefault(e => e.Id == id);
        if (existingEvent != null)
        {
            _events.Remove(existingEvent);
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<Event> GetAll()
    {
        return _events.AsReadOnly();
    }

    /// <inheritdoc />
    public Event? GetById(int id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }

    /// <inheritdoc />
    public bool Update(int id, Event updatedEvent)
    {
        var existingEvent = _events.FirstOrDefault(e => e.Id == id);
        if (existingEvent != null)
        {
            existingEvent.Title = updatedEvent.Title;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.StartAt = updatedEvent.StartAt;
            existingEvent.EndAt = updatedEvent.EndAt;
            return true;
        }
        return false;
    }
}
