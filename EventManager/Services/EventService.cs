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
        var existingEvent = _eventRepository.GetAll().FirstOrDefault(e => e.Id == newEventDto.Id);
        if (existingEvent == null)
        {
            _eventRepository.Add(EventMapper.ToEvent(newEventDto));
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public bool DeleteEvent(int id)
    {
        var existingEvent = _eventRepository.GetAll().FirstOrDefault(e => e.Id == id);
        if (existingEvent != null)
        {
            _eventRepository.Delete(existingEvent);
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public PaginatedResultDto GetAllEvents(GetQuery query)
    {
        IEnumerable<Event> events = _eventRepository.GetAll();

        // Фильтрация
        if (!string.IsNullOrEmpty(query.Title))
            events = events.Where(e => e.Title.Contains(query.Title, StringComparison.OrdinalIgnoreCase));

        if (query.From.HasValue)
            events = events.Where(e => e.StartAt >= query.From.Value);

        if (query.To.HasValue)
            events = events.Where(e => e.EndAt <= query.To.Value);

        return new PaginatedResultDto()
        {
            Events = events.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).Select(EventMapper.ToEventDto), // Пагинация
            TotalCount = events.Count(),
            PageSize = query.PageSize,
            Page = query.Page
        };
    }

    /// <inheritdoc/>
    public EventDto? GetEvent(int id)
    {
        var eventById = _eventRepository.GetAll().FirstOrDefault(e => e.Id == id);
        return eventById == null ? null : EventMapper.ToEventDto(eventById);
    }

    /// <inheritdoc/>
    public bool UpdateEvent(int id, EventDto updatedEventDto)
    {
        var existingEvent = _eventRepository.GetAll().FirstOrDefault(e => e.Id == id);
        if (existingEvent != null)
        {
            _eventRepository.Update(id, EventMapper.ToEvent(updatedEventDto));
            return true;
        }
        return false;
    }
}
