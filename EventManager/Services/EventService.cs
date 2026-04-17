using EventManager.Interfaces;
using EventManager.Models;

namespace EventManager.Services;

/// <inheritdoc/>
public class EventService(IEventRepository eventRepository) : IEventService
{
    private readonly IEventRepository _eventRepository = eventRepository;

    /// <inheritdoc/>
    public void CreateEvent(EventDto newEventDto)
    {
        var existingEvent = _eventRepository.GetById(newEventDto.Id);
        if (existingEvent != null)
            throw new InvalidOperationException($"Событие с id {newEventDto.Id} уже существует.");
        _eventRepository.Add(EventMapper.ToEvent(newEventDto));
    }

    /// <inheritdoc/>
    public void DeleteEvent(int id)
    {
        var existingEvent = _eventRepository.GetById(id);
        if (existingEvent == null)
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
        _eventRepository.Delete(existingEvent);
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
    public EventDto GetEvent(int id)
    {
        var eventById = _eventRepository.GetById(id);
        if (eventById == null)
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
        return EventMapper.ToEventDto(eventById);
    }

    /// <inheritdoc/>
    public void UpdateEvent(int id, EventPutDto updatedEventDto)
    {
        var existingEvent = _eventRepository.GetById(id);
        if (existingEvent == null)
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");

        // Использую Dto, чтобы спровоцировать валидацию в свойствах. Это ок?
        EventDto updateEventDto = new()
        {
            Id = id,
            Title = updatedEventDto.Title ?? existingEvent.Title,
            Description = updatedEventDto.Description ?? existingEvent.Description,
            StartAt = updatedEventDto.StartAt ?? existingEvent.StartAt,
            EndAt = updatedEventDto.EndAt ?? existingEvent.EndAt
        };
        _eventRepository.Update(id, EventMapper.ToEvent(updateEventDto));
    }
}
