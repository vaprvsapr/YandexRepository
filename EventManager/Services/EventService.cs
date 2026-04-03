using EventManager.Interfaces;
using EventManager.Models;

namespace EventManager.Services;

/// <inheritdoc/>
public class EventService(IEventRepository eventRepository) : IEventService
{
    private readonly IEventRepository _eventRepository = eventRepository;

    /// <inheritdoc/>
    public bool CreateEvent(EventDto newEventDto)
    {
        return _eventRepository.Add(EventDto.ToEvent(newEventDto));
    }

    /// <inheritdoc/>
    public bool DeleteEvent(int id)
    {
        return _eventRepository.Delete(id);
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<EventDto> GetAllEvents()
    {
        return _eventRepository.GetAll().Select(EventDto.ToEventDto).ToList().AsReadOnly();
    }

    /// <inheritdoc/>
    public EventDto? GetEvent(int id)
    {
        var eventById = _eventRepository.GetById(id);
        return eventById == null ? null : EventDto.ToEventDto(eventById);
    }

    /// <inheritdoc/>
    public bool UpdateEvent(int id, EventDto updatedEventDto)
    {
        return _eventRepository.Update(id, EventDto.ToEvent(updatedEventDto));
    }
}
