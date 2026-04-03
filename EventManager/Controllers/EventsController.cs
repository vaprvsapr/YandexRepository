using EventManager.Interfaces;
using EventManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventManager.Controllers;

/// <summary>
/// Предоставляет HTTP API для управления событиями, включая получение, создание, обновление и удаление событий.
/// </summary>
/// <remarks>
/// Этот контроллер реализует стандартные CRUD-операции для сущности события. Все методы возвращают результат в унифицированном формате ApiResult.
/// </remarks>
/// <param name="eventService">Сервис, реализующий бизнес-логику для операций с событиями.</param>
[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    private readonly IEventService _eventService = eventService;

    /// <summary>
    /// Возвращает коллекцию всех доступных событий.
    /// </summary>
    /// <returns>Объект ApiResult, содержащий коллекцию событий.</returns>
    /// <response code="200">Возвращается JSON-структура ApiResult с деталями ответа и HTTP статус-кодом 200 OK.</response>
    [ProducesResponseType(typeof(ApiResult<IReadOnlyCollection<EventDto>>), (int)HttpStatusCode.OK)]
    [Produces("application/json")]
    [HttpGet]
    public ApiResult<IReadOnlyCollection<EventDto>> GetAllEvents()
    {
        var events = _eventService.GetAllEvents();
        return new ApiResult<IReadOnlyCollection<EventDto>>
        {
            Data = events,
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = $"Получено событий: {events.Count}",
            DateTime = DateTime.Now,
        };
    }

    /// <summary>
    /// Возвращает событие по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор события, которое требуется получить.</param>
    /// <returns>Объект ApiResult, содержащий данные найденного события, либо информацию об ошибке.</returns>
    /// <response code="200">Возвращается JSON-структура ApiResult с деталями ответа и HTTP статус-кодом 200 OK, если событие найдено.</response>
    /// <response code="400">Возвращается JSON-структура ApiResult с деталями ответа и HTTP статус-кодом 400 Bad Request, если событие не найдено.</response>
    [ProducesResponseType(typeof(ApiResult<EventDto?>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResult<EventDto?>), (int)HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [HttpGet("{id:int}")]
    public ApiResult<EventDto?> GetEventById([FromRoute] int id)
    {
        var eventById = _eventService.GetEvent(id);
        return new ApiResult<EventDto?>
        {
            Data = eventById,
            Success = eventById != null,
            StatusCode = eventById != null ? HttpStatusCode.OK : HttpStatusCode.NotFound,
            Message = eventById != null ? $"Получено событие с id: {id}" : $"Не найдено событие с id: {id}",
            DateTime = DateTime.Now,
        };
    }

    /// <summary>
    /// Создает новое событие на основе предоставленных данных.
    /// </summary>
    /// <param name="newEvent">Данные нового события, которые необходимо создать.</param>
    /// <returns>Объект ApiResult с информацией об успешности создания события.</returns>
    /// <response code="201">Возвращается JSON-структура ApiResult с деталями ответа и HTTP статус-кодом 201 Created, если событие успешно создано.</response>
    /// <response code="409">Возвращается JSON-структура ApiResult с деталями ответа и HTTP статус-кодом 409 Conflict, если не удалось создать событие.</response>
    [ProducesResponseType(typeof(ApiResult), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ApiResult), (int)HttpStatusCode.Conflict)]
    [Produces("application/json")]
    [HttpPost]
    public ApiResult PostEvent([FromBody] EventDto newEvent)
    {
        var isPosted = _eventService.CreateEvent(newEvent);
        return new ApiResult
        {
            Success = isPosted,
            StatusCode = isPosted ? HttpStatusCode.Created : HttpStatusCode.Conflict,
            Message = isPosted ? $"Создано событие с id: {newEvent.Id}" : $"Не удалось создать событие с id: {newEvent.Id}",
            DateTime = DateTime.Now,
        };
    }

    /// <summary>
    /// Обновляет существующее событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события, которое требуется обновить.</param>
    /// <param name="updatedEvent">Новые данные события.</param>
    /// <returns>Объект ApiResult с информацией об успешности обновления события.</returns>
    /// <response code="200">Возвращается JSON-структура ApiResult с деталями ответа и HTTP статус-кодом 200 OK, если событие успешно обновлено.</response>
    /// <response code="409">Возвращается JSON-структура ApiResult с деталями ответа и HTTP статус-кодом 409 Conflict, если событие не найдено.</response>
    [ProducesResponseType(typeof(ApiResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResult), (int)HttpStatusCode.Conflict)]
    [Produces("application/json")]
    [HttpPut("{id:int}")]
    public ApiResult PutEvent([FromRoute] int id, [FromBody] EventDto updatedEvent)
    {
        var isUpdated = _eventService.UpdateEvent(id, updatedEvent);
        return new ApiResult
        {
            Success = isUpdated,
            StatusCode = isUpdated ? HttpStatusCode.OK : HttpStatusCode.Conflict,
            Message = isUpdated ? $"Обновлено событие с id: {id}" : $"Не найдено событие с id: {id}",
            DateTime = DateTime.Now,
        };
    }

    /// <summary>
    /// Удаляет событие с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор события, которое требуется удалить.</param>
    /// <returns>Объект ApiResult с информацией об успешности удаления события.</returns>
    /// <response code="200">Возвращается JSON-структура ApiResult с деталями ответа и HTTP статус-кодом 200 OK, если событие успешно удалено.</response>
    /// <response code="404">Возвращается JSON-структура ApiResult с деталями ответа и HTTP статус-кодом 404 Not Found, если событие не найдено.</response>
    [ProducesResponseType(typeof(ApiResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResult), (int)HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [HttpDelete("{id:int}")]
    public ApiResult Delete([FromRoute] int id)
    {
        var isDeleted = _eventService.DeleteEvent(id);
        return new ApiResult
        {
            Success = isDeleted,
            StatusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotFound,
            Message = isDeleted ? $"Удалено событие с id: {id}" : $"Не найдено событие с id: {id}",
            DateTime = DateTime.Now,
        };
    }
}
