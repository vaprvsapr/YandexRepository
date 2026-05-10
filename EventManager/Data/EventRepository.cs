using EventManager.Interfaces;
using EventManager.Models.Events;

namespace EventManager.Data;

/// <inheritdoc/>
public class EventRepository : IRepository<Event>
{
    private readonly List<Event> _events = [];


    /// <summary>
    /// Добавляет новое событие в репозиторий.
    /// </summary>
    /// <param name="eventToAdd">Событие, которое требуется добавить.</param>
    public void Add(Event eventToAdd) => _events.Add(eventToAdd);


    /// <summary>
    /// Удаляет событие из репозитория.
    /// </summary>
    /// <param name="eventToDelete">Событие, которое требуется удалить.</param>
    public void Delete(Event eventToDelete) => _events.Remove(eventToDelete);



    /// <summary>
    /// Возвращает неизменяемую коллекцию всех событий.
    /// </summary>
    /// <returns>Коллекция объектов <see cref="Event"/>. Если события отсутствуют, возвращается пустая коллекция.</returns>
    public IReadOnlyCollection<Event> GetAll()
    {
        return _events.AsReadOnly();
    }


    /// <summary>
    /// Возвращает событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns>Объект <see cref="Event"/>, если событие найдено; в противном случае — <see langword="null"/>.</returns>
    public Event? GetById(Guid id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }


    /// <summary>
    /// Обновляет существующее событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <param name="updatedEvent">Новые данные события.</param>
    public void Update(Guid id, Event updatedEvent)
    {
        Event eventToBeUpdated = _events.FirstOrDefault(e => e.Id == id) ?? 
            throw new InvalidOperationException($"Event with id {id} not found.");

        // Полностью обновляем все поля, включая Id.
        eventToBeUpdated.Id = updatedEvent.Id;
        eventToBeUpdated.Title = updatedEvent.Title;
        eventToBeUpdated.Description = updatedEvent.Description;
        eventToBeUpdated.StartAt = updatedEvent.StartAt;
        eventToBeUpdated.EndAt = updatedEvent.EndAt;
    }
}
