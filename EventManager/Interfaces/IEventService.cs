using EventManager.Models.Events;
using EventManager.Models.Queries;

namespace EventManager.Interfaces;

/// <summary>
/// Сервис для бизнес-логики управления событиями, предоставляющий методы для получения, создания, обновления и удаления событий.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Возвращает событие по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns>Объект <see cref="EventDto"/>, если событие найдено; в противном случае — <see langword="null"/>.</returns>
    public EventDto? GetEvent(Guid id);

    /// <summary>
    /// Возвращает коллекцию всех доступных событий.
    /// </summary>
    /// <returns>Коллекция объектов <see cref="EventDto"/>. Если события отсутствуют, возвращается пустая коллекция.</returns>
    public PaginatedResultDto GetAllEvents(GetQuery query);

    /// <summary>
    /// Создает новое событие на основе предоставленных данных.
    /// </summary>
    /// <param name="newEvent">Данные нового события.</param>
    /// <returns><see langword="true"/>, если событие успешно создано; в противном случае — <see langword="false"/>.</returns>
    public void CreateEvent(EventDto newEvent);

    /// <summary>
    /// Обновляет существующее событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <param name="updatedEventDto">Новые данные события.</param>
    /// <returns><see langword="true"/>, если событие успешно обновлено; в противном случае — <see langword="false"/>.</returns>
    public void UpdateEvent(Guid id, EventDto updatedEventDto);

    /// <summary>
    /// Удаляет событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns><see langword="true"/>, если событие успешно удалено; в противном случае — <see langword="false"/>.</returns>
    public void DeleteEvent(Guid id);
}
