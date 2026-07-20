using EventManager.Application.Repositories;
using EventManager.Application.Dto;
using EventManager.Application.Queries;
using EventManager.Application.Mappers;
using EventManager.Domain.Models;
using Microsoft.Extensions.Logging;
using EventManager.Application.Services.Interfaces;

namespace EventManager.Application.Services;

/// <summary>
/// Сервис для управления событиями, реализующий бизнес-логику создания, получения, обновления и удаления событий,
/// а также получения списка событий с поддержкой фильтрации и пагинации. 
/// В качестве хранилища используется база данных через AppDbContext, 
/// а для логирования используется ILogger.
/// </summary>
/// <param name="eventRepository"></param>
/// <param name="logger"></param>
public class EventService(
    IEventRepository eventRepository, 
    ILogger<EventService> logger) : IEventService
{
    private readonly ILogger<EventService> _logger = logger;
    private readonly IEventRepository _eventRepository = eventRepository;

    /// <inheritdoc/>
    public async Task<EventInfoDto> CreateEvent(EventCreateDto eventCreateDto)
    {
        var newEvent = new Event 
        { 
            Id = eventCreateDto.Id,
            Title = eventCreateDto.Title,
            Description = eventCreateDto.Description,
            StartAt = eventCreateDto.StartAt,
            EndAt = eventCreateDto.EndAt,
            TotalSeats = eventCreateDto.TotalSeats
        };
        newEvent = await _eventRepository.CreateAsync(newEvent);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Event created: {title} with id: {id}", newEvent.Title, newEvent.Id);
        return EventMapper.ToEventInfoDto(newEvent);
    }

    /// <inheritdoc/>
    public async Task DeleteEvent(Guid id)
    {
        await _eventRepository.DeleteByIdAsync(id);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Event with {id} was deleted.", id);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResultDto> GetAllEvents(GetEventQuery getQuery)
    {
        IQueryable<Event> events = _eventRepository.GetAll();

        // Фильтрация
        if (getQuery.From.HasValue)
            events = events.Where(e => e.StartAt >= getQuery.From.Value.ToUniversalTime());

        if (getQuery.To.HasValue)
            events = events.Where(e => e.EndAt <= getQuery.To.Value.ToUniversalTime());

        if (!string.IsNullOrEmpty(getQuery.Title))
            events = events
                .AsEnumerable()
                .Where(e => e.Title.Contains(getQuery.Title, StringComparison.OrdinalIgnoreCase))
                .AsQueryable();

        return new PaginatedResultDto()
        {
            Events = events
            .Skip((getQuery.Page - 1) * getQuery.PageSize)
            .Take(getQuery.PageSize)
            .Select(EventMapper.ToEventInfoDto), // Пагинация
            TotalCount = events.Count(),
            PageSize = getQuery.PageSize,
            Page = getQuery.Page
        };
    }

    /// <inheritdoc/>
    public async Task<EventInfoDto> GetEvent(Guid id)
    {
        var existingEvent = await _eventRepository.GetByIdAsync(id);
        return EventMapper.ToEventInfoDto(existingEvent);
    }

    /// <inheritdoc/>
    public async Task<EventInfoDto> UpdateEvent(Guid id, EventUpdateDto updatedEventDto)
    {
        Event @event = EventMapper.ToEvent(updatedEventDto, id);
        var updatedEvent = await _eventRepository.UpdateAsync(@event);
        return EventMapper.ToEventInfoDto(updatedEvent);
    }
}
