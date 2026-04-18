using EventManager.Interfaces;
using EventManager.Models;

namespace EventManager.Data;

/// <inheritdoc/>
public class EventRepository : IEventRepository
{
    private readonly List<Event> _events = [];

    /// <inheritdoc />
    public void Add(Event eventToAdd) => _events.Add(eventToAdd);

    /// <inheritdoc />
    public void Delete(Event eventToDelete) => _events.Remove(eventToDelete);


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
    public void Update(int id, Event updatedEvent)
    {
        Event eventToBeUpdated = _events.FirstOrDefault(e => e.Id == id) ?? 
            throw new InvalidOperationException($"Event with id {id} not found.");
        eventToBeUpdated = updatedEvent;
    }
}
