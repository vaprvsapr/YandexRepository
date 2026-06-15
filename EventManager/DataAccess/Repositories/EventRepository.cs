using EventManager.Models.Events;
using EventManager.Services;
using Microsoft.EntityFrameworkCore;

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
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<Event> CreateAsync(EventCreateDto eventCreateDto)
    {
        var existingEvent = await _context.Events.FindAsync(eventCreateDto.Id);
        if (existingEvent is not null)
            throw new InvalidOperationException($"Событие с id {eventCreateDto.Id} уже существует.");
        var newEvent = EventMapper.ToEvent(eventCreateDto);
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return newEvent;
    }

    /// <summary>
    /// Удаляет событие по его уникальному идентификатору. Проверяет, что событие с указанным ID существует, и удаляет его из базы данных.
    /// Выбрасывает исключение, если событие с указанным ID не найдено.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task DeleteByIdAsync(Guid id)
    {
        var existingEvent = await _context.Events.FindAsync(id) ??
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
        _context.Events.Remove(existingEvent);
        await _context.SaveChangesAsync();
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
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<Event> GetByIdAsync(Guid id)
    {
        var existingEvent = await _context.Events.FindAsync(id) ??
            throw new KeyNotFoundException($"Событие с id {id} не найдено.");
        return existingEvent;
    }

    /// <summary>
    /// Обновляет существующее событие на основе предоставленных данных. Проверяет, что событие с указанным ID существует, и обновляет его поля.
    /// Выбрасывает исключение, если событие с указанным ID не найдено. Сохраняет изменения в базе данных и возвращает обновленное событие.
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<Event> UpdateAsync(Event @event)
    {
        var existingEvent = await _context.Events.FindAsync(@event.Id) ??
            throw new KeyNotFoundException($"Событие с id {@event.Id} не найдено.");
        existingEvent.Title = @event.Title;
        existingEvent.Description = @event.Description;
        existingEvent.StartAt = @event.StartAt;
        existingEvent.EndAt = @event.EndAt;
        existingEvent.TotalSeats = @event.TotalSeats;
        await _context.SaveChangesAsync();
        return existingEvent;
    }
}
