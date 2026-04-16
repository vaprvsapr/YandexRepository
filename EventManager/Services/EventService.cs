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
    public PaginatedResultDto GetAllEvents(GetQuery query)
    {
        IEnumerable<Event> events = _eventRepository.GetAll();

        // Заранее находим количество событий до фильтрации для корректной пагинации
        int totalCount = events.Count();

        // Фильтрация
        if (!string.IsNullOrEmpty(query.Title))
            events = events.Where(e => e.Title.Contains(query.Title, StringComparison.OrdinalIgnoreCase));

        if (query.From.HasValue)
            events = events.Where(e => e.StartAt >= query.From.Value);

        if (query.To.HasValue)
            events = events.Where(e => e.EndAt <= query.To.Value);

        // Пагинация
        events = events.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);

        return new PaginatedResultDto()
        { 
            Events = events.Select(EventDto.ToEventDto), 
            TotalCount = totalCount, 
            PageSize = query.PageSize, 
            Page = query.Page 
        };
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
