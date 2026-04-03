using EventManager.Models;

namespace EventManager.Interfaces;

/// <summary>
/// Репозиторий для управления данными событий, предоставляющий методы для получения, добавления, обновления и удаления событий.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Возвращает событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns>Объект <see cref="Event"/>, если событие найдено; в противном случае — <see langword="null"/>.</returns>
    Event? GetById(int id);

    /// <summary>
    /// Возвращает неизменяемую коллекцию всех событий.
    /// </summary>
    /// <returns>Коллекция объектов <see cref="Event"/>. Если события отсутствуют, возвращается пустая коллекция.</returns>
    IReadOnlyCollection<Event> GetAll();

    /// <summary>
    /// Добавляет новое событие в репозиторий.
    /// </summary>
    /// <param name="newEvent">Событие, которое требуется добавить.</param>
    /// <returns><see langword="true"/>, если событие успешно добавлено; в противном случае — <see langword="false"/>.</returns>
    bool Add(Event newEvent);

    /// <summary>
    /// Обновляет существующее событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <param name="updatedEvent">Новые данные события.</param>
    /// <returns><see langword="true"/>, если событие успешно обновлено; в противном случае — <see langword="false"/>.</returns>
    bool Update(int id, Event updatedEvent);

    /// <summary>
    /// Удаляет событие с указанным идентификатором из репозитория.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns><see langword="true"/>, если событие успешно удалено; в противном случае — <see langword="false"/>.</returns>
    bool Delete(int id);
}
