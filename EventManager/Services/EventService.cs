using EventManager.DataAccess;
using EventManager.Interfaces;
using EventManager.Models.Events;
using EventManager.Models.Queries;

namespace EventManager.Services;


/// <summary>
/// Сервис для управления событиями, реализующий бизнес-логику создания, получения, обновления и удаления событий,
/// а также получения списка событий с поддержкой фильтрации и пагинации. 
/// В качестве хранилища используется база данных через AppDbContext, 
/// а для логирования используется ILogger.
/// </summary>
/// <param name="context"></param>
/// <param name="logger"></param>
public class EventService(AppDbContext context, ILogger<EventService> logger) : IEventService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<EventService> _logger = logger;

    /// <inheritdoc/>
    public async Task<EventInfoDto> CreateEvent(EventCreateDto eventCreateDto)
    {
        var existingEvent = await _context.Events.FindAsync(eventCreateDto.Id);
        if (existingEvent is not null)
            throw new InvalidOperationException($"Событие с id {eventCreateDto.Id} уже существует.");
        var newEvent = EventMapper.ToEvent(eventCreateDto);
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Event created: {title} with id: {id}", newEvent.Title, newEvent.Id);
        return EventMapper.ToEventInfoDto(newEvent);
    }

    /// <inheritdoc/>
    public async Task DeleteEvent(Guid id)
    {
        var existingEvent = await _context.Events.FindAsync(id) ??
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
        _context.Events.Remove(existingEvent);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Event deleted: {title} with id: {id}", existingEvent.Title, existingEvent.Id);

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<PaginatedResultDto> GetAllEvents(GetQuery getQuery)
    {
        IEnumerable<Event> events = _context.Events.AsEnumerable();

        // Фильтрация
        if (!string.IsNullOrEmpty(getQuery.Title))
            events = events.Where(e => e.Title.Contains(getQuery.Title, StringComparison.OrdinalIgnoreCase));

        if (getQuery.From.HasValue)
            events = events.Where(e => e.StartAt >= getQuery.From.Value);

        if (getQuery.To.HasValue)
            events = events.Where(e => e.EndAt <= getQuery.To.Value);

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
    public async Task<EventInfoDto?> GetEvent(Guid id)
    {
        var existingEvent = await _context.Events.FindAsync(id) ??
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
        return EventMapper.ToEventInfoDto(existingEvent);
    }

    /// <inheritdoc/>
    public async Task<EventInfoDto> UpdateEvent(Guid id, EventUpdateDto updatedEventDto)
    {
        var existingEvent = await _context.Events.FindAsync(id) ??
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
        existingEvent.Title = updatedEventDto.Title;
        existingEvent.Description = updatedEventDto.Description;
        existingEvent.StartAt = updatedEventDto.StartAt;
        existingEvent.EndAt = updatedEventDto.EndAt;
        existingEvent.TotalSeats = updatedEventDto.TotalSeats;
        await _context.SaveChangesAsync();
        return EventMapper.ToEventInfoDto(existingEvent);
    }
}
