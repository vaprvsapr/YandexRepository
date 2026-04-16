using EventManager.Interfaces;
using EventManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventManager.Controllers;

/// <summary>
/// Предоставляет HTTP API для управления событиями, включая получение, создание, обновление и удаление событий.
/// </summary>
/// <remarks>
/// Этот контроллер реализует стандартные CRUD-операции для сущности события. Все методы возвращают результат посредством ActionResult.
/// </remarks>
/// <param name="eventService">Сервис, реализующий бизнес-логику для операций с событиями.</param>
[ApiController]
[Route("events")]
public class EventsController(IEventService eventService) : ControllerBase
{
    private readonly IEventService _eventService = eventService;

    /// <summary>
    /// Возвращает коллекцию всех доступных событий.
    /// </summary>
    /// <returns>Коллекция событий.</returns>
    /// <response code="200">Возвращается успешный ответ с коллекцией событий и HTTP статус-кодом 200 OK.</response>
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [Produces("application/json")]
    [HttpGet]
    public ActionResult<IReadOnlyCollection<EventDto>> GetAllEvents([FromQuery] GetQuery query)
    {
        var events = _eventService.GetAllEvents(query);
        return Ok(events);
    }

    /// <summary>
    /// Возвращает событие по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор события, которое требуется получить.</param>
    /// <returns>Данные найденного события.</returns>
    /// <response code="200">Возвращается успешный ответ с данными события и HTTP статус-кодом 200 OK.</response>
    /// <response code="404">Возвращается HTTP статус-код 404 Not Found, если событие не найдено.</response>
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [HttpGet("{id:int}")]
    public ActionResult<EventDto> GetEventById([FromRoute] int id)
    {
        var eventById = _eventService.GetEvent(id);
        return eventById is not null ? Ok(eventById) : NotFound();
    }

    /// <summary>
    /// Создает новое событие на основе предоставленных данных.
    /// </summary>
    /// <param name="newEvent">Данные нового события, которые необходимо создать.</param>
    /// <returns>Информация о созданном событии.</returns>
    /// <response code="201">Возвращается успешный ответ с данными созданного события и HTTP статус-кодом 201 Created.</response>
    /// <response code="400">Возвращается HTTP статус-код 400 Bad Request, если не удалось создать событие или обнаружены ошибки валидации.</response>
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [Produces("application/json")]
    [HttpPost]
    public ActionResult<EventDto> PostEvent([FromBody] EventDto newEvent)
    {
        var isPosted = _eventService.CreateEvent(newEvent);
        return isPosted ? CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent) : BadRequest();
    }

    /// <summary>
    /// Обновляет существующее событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события, которое требуется обновить.</param>
    /// <param name="updatedEvent">Новые данные события.</param>
    /// <returns>Результат обновления события.</returns>
    /// <response code="200">Возвращается HTTP статус-код 200 OK, если событие успешно обновлено.</response>
    /// <response code="404">Возвращается HTTP статус-код 404 Not Found, если не удалось обновить событие.</response>
    /// <response code="400">Возвращается HTTP статус-код 400 Bad Request, если были обнаружены ошибки валидации.</response>
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [Produces("application/json")]
    [HttpPut("{id:int}")]
    public ActionResult PutEvent([FromRoute] int id, [FromBody] EventDto updatedEvent)
    {
        var isUpdated = _eventService.UpdateEvent(id, updatedEvent);
        return isUpdated ? Ok() : NotFound();
    }

    /// <summary>
    /// Удаляет событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события, которое требуется удалить.</param>
    /// <returns>Результат удаления события.</returns>
    /// <response code="204">Возвращается HTTP статус-код 204 No Content, если событие успешно удалено.</response>
    /// <response code="404">Возвращается HTTP статус-код 404 Not Found, если событие не найдено или не удалено.</response>
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [HttpDelete("{id:int}")]
    public ActionResult Delete([FromRoute] int id)
    {
        var isDeleted = _eventService.DeleteEvent(id);
        return isDeleted ? NoContent() : NotFound();
    }
}
