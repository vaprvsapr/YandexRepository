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
        var existingEvent = await _eventRepository.GetByIdAsync(eventCreateDto.Id);
        if (existingEvent != null)
            throw new InvalidOperationException($"Событие с ID:{eventCreateDto.Id} уже существует.");

        var newEvent = new Event 
        { 
            Id = eventCreateDto.Id,
            Title = eventCreateDto.Title,
            Description = eventCreateDto.Description,
            StartAt = eventCreateDto.StartAt,
            EndAt = eventCreateDto.EndAt,
            TotalSeats = eventCreateDto.TotalSeats
        };
        await _eventRepository.CreateAsync(newEvent);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Event created: {title} with id: {id}", newEvent.Title, newEvent.Id);

        return await GetEvent(eventCreateDto.Id);
    }

    /// <inheritdoc/>
    public async Task DeleteEvent(Guid id)
    {
        var existingEvent = await GetEventByIdAsync(id);

        await _eventRepository.DeleteAsync(existingEvent);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Event with ID:{id} was deleted.", id);
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
        return EventMapper.ToEventInfoDto(await GetEventByIdAsync(id));
    }

    /// <inheritdoc/>
    public async Task<EventInfoDto> UpdateEvent(Guid id, EventUpdateDto eventUpdateDto)
    {
        var existingEvent = await GetEventByIdAsync(id);

        existingEvent.Title = eventUpdateDto.Title ?? existingEvent.Title;
        existingEvent.Description = eventUpdateDto.Description ?? existingEvent.Description;
        existingEvent.StartAt = eventUpdateDto.StartAt ?? existingEvent.StartAt;
        existingEvent.EndAt = eventUpdateDto.EndAt ?? existingEvent.EndAt;

        await _eventRepository.UpdateAsync(existingEvent);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Event with ID:{id} was updated.", id);

        return await GetEvent(id);
    }

    private async Task<Event> GetEventByIdAsync(Guid id)
    {
        return await _eventRepository.GetByIdAsync(id) ??
            throw new KeyNotFoundException($"Событие с ID:{id} не найдено.");
    }
}
