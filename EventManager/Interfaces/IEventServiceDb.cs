using EventManager.Models.Events;
using EventManager.Models.Queries;

namespace EventManager.Interfaces;

/// <summary>
/// Сервис для бизнес-логики управления событиями, предоставляющий методы для получения, создания, обновления и удаления событий.
/// </summary>
public interface IEventServiceDb
{
    /// <summary>
    /// Возвращает событие по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns>Объект <see cref="EventInfoDto"/>, если событие найдено; в противном случае — <see langword="null"/>.</returns>
    public Task<EventInfoDto?> GetEvent(Guid id);

    /// <summary>
    /// Возвращает коллекцию всех доступных событий.
    /// </summary>
    /// <returns>Коллекция объектов <see cref="EventInfoDto"/>. Если события отсутствуют, возвращается пустая коллекция.</returns>
    public Task<PaginatedResultDto> GetAllEvents(GetQuery query);

    /// <summary>
    /// Создает новое событие на основе предоставленных данных.
    /// </summary>
    /// <param name="eventCreateDto">Данные нового события.</param>
    /// <returns>Объект <see cref="EventInfoDto"/>, если событие успешно создано.</returns>
    public Task<EventInfoDto> CreateEvent(EventCreateDto eventCreateDto);

    /// <summary>
    /// Обновляет существующее событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <param name="eventUpdateDto">Новые данные события.</param>
    /// <returns>Объект <see cref="EventInfoDto"/>, если событие успешно обновлено.</returns>
    public Task<EventInfoDto> UpdateEvent(Guid id, EventUpdateDto eventUpdateDto);

    /// <summary>
    /// Удаляет событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns><see langword="true"/>, если событие успешно удалено; в противном случае — <see langword="false"/>.</returns>
    public Task DeleteEvent(Guid id);
}
