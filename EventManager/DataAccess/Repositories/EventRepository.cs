using EventManager.Models.Events;

namespace EventManager.DataAccess.Repositories;

/// <summary>
/// Репозиторий для управления событиями, обеспечивающий операции создания, получения, обновления и удаления событий.
/// </summary>
/// <param name="context"></param>
public class EventRepository(AppDbContext context) : IEventRepository
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Создает новое событие на основе предоставленных данных. Проверяет, что событие с таким ID еще не существует, и сохраняет его в базе данных.
    /// Выбрасывает исключение, если событие с указанным ID уже существует.
    /// </summary>
    /// <param name="eventCreateDto"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<Event> CreateAsync(EventCreateDto eventCreateDto, CancellationToken ct = default)
    {
        var existingEvent = await _context.Events.FindAsync([ eventCreateDto.Id ], cancellationToken: ct);
        if (existingEvent is not null)
            throw new InvalidOperationException($"Событие с id {eventCreateDto.Id} уже существует.");
        var newEvent = EventMapper.ToEvent(eventCreateDto);
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync(ct);

        return newEvent;
    }

    /// <summary>
    /// Удаляет событие по его уникальному идентификатору. Проверяет, что событие с указанным ID существует, и удаляет его из базы данных.
    /// Выбрасывает исключение, если событие с указанным ID не найдено.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task DeleteByIdAsync(Guid id, CancellationToken ct = default)
    {
        var existingEvent = await GetByIdAsync(id, ct);
        _context.Events.Remove(existingEvent);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Возвращает все события в виде IQueryable, позволяя выполнять дальнейшие операции фильтрации, сортировки и проекции на уровне базы данных.
    /// </summary>
    /// <returns></returns>
    public IQueryable<Event> GetAll()
    {
        return _context.Events;
    }

    /// <summary>
    /// Возвращает событие по его уникальному идентификатору. Проверяет, что событие с указанным ID существует, и возвращает его.
    /// Выбрасывает исключение, если событие с указанным ID не найдено.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<Event> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Events.FindAsync([ id ], cancellationToken: ct) ?? 
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
    }

    /// <summary>
    /// Обновляет существующее событие на основе предоставленных данных. Проверяет, что событие с указанным ID существует, и обновляет его поля.
    /// Выбрасывает исключение, если событие с указанным ID не найдено. Сохраняет изменения в базе данных и возвращает обновленное событие.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<Event> UpdateAsync(Event @event, CancellationToken ct = default)
    {
        var existingEvent = await GetByIdAsync(@event.Id, ct);

        existingEvent.Title = @event.Title;
        existingEvent.Description = @event.Description;
        existingEvent.StartAt = @event.StartAt;
        existingEvent.EndAt = @event.EndAt;
        existingEvent.TotalSeats = @event.TotalSeats;

        await _context.SaveChangesAsync(ct);
        return existingEvent;
    }
}
