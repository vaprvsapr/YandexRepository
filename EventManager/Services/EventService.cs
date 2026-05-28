using EventManager.Interfaces;
using EventManager.Models.Events;
using EventManager.Models.Queries;


namespace EventManager.Services;

/// <inheritdoc/>
public class EventService(IRepository<Event> eventRepository, ILogger<EventService> logger) : IEventService
{
    private readonly IRepository<Event> _eventRepository = eventRepository;
    private readonly ILogger<EventService> _logger = logger;

    /// <inheritdoc/>
    public EventInfoDto CreateEvent(EventCreateDto newEventDto)
    {
        var existingEvent = _eventRepository.GetById(newEventDto.Id);
        if (existingEvent != null)
            throw new InvalidOperationException($"Событие с id {newEventDto.Id} уже существует.");
        Event newEvent = EventMapper.ToEvent(newEventDto);
        _eventRepository.Add(newEvent);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Event created: {title} with id: {id}", newEventDto.Title, newEventDto.Id);
        return EventMapper.ToEventInfoDto(newEvent);
    }

    /// <inheritdoc/>
    public void DeleteEvent(Guid id)
    {
        var existingEvent = _eventRepository.GetById(id) ?? 
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
        _eventRepository.Delete(existingEvent);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Event deleted: {title} with id: {id}", existingEvent.Title, existingEvent.Id);
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
            Events = events.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).Select(EventMapper.ToEventInfoDto), // Пагинация
            TotalCount = events.Count(),
            PageSize = query.PageSize,
            Page = query.Page
        };
    }

    /// <inheritdoc/>
    public EventInfoDto GetEvent(Guid id)
    {
        var eventById = _eventRepository.GetById(id) ?? 
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
        return EventMapper.ToEventInfoDto(eventById);
    }

    /// <inheritdoc/>
    public EventInfoDto UpdateEvent(Guid id, EventUpdateDto updatedEventDto)
    {
        _ = _eventRepository.GetById(id) ??
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");

        Event updatedEvent = EventMapper.ToEvent(updatedEventDto, id);
        _eventRepository.Update(id, updatedEvent);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Event updated: {title} with id: {id}", updatedEventDto.Title, id);
        return EventMapper.ToEventInfoDto(updatedEvent);
    }
}
