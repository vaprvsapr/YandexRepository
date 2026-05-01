using EventManager.Interfaces;
using EventManager.Models.Bookings;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventManager.Controllers;

/// <summary>
/// Предоставляет HTTP API для управления бронированиями событий.
/// </summary>
/// <remarks>
/// Контроллер реализует операции для создания, получения одного и списка бронирований.
/// </remarks>
/// <param name="bookingService">Сервис, реализующий бизнес-логику для операций с бронированиями.</param>
[ApiController]
[Route("bookings")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    private readonly IBookingService _bookingService = bookingService;

    /// <summary>
    /// Размещает бронирование для события по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор события, для которого создаётся бронирование.</param>
    /// <returns>Информация о созданном бронировании.</returns>
    /// <response code="202">Бронирование принято к обработке.</response>
    /// <response code="404">Событие не найдено.</response>
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [Route("~/events/{id}/book")]
    [HttpPost]
    public ActionResult<BookingDto> Book([FromRoute] Guid id)
    {
        BookingDto createdBooking = _bookingService.CreateBookingAsync(id);
        return Accepted($"/bookings/{createdBooking.Id}", createdBooking);
    }

    /// <summary>
    /// Возвращает список бронирований для события по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns>Список бронирований для события.</returns>
    /// <response code="200">Список успешно возвращён.</response>
    /// <response code="404">Событие не найдено.</response>
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [HttpGet("event/{id:guid}")]
    public ActionResult<List<BookingDto>> GetBookingsByEventId([FromRoute] Guid id)
    {
        return Ok(_bookingService.GetBookingsByEventIdAsync(id));
    }

    /// <summary>
    /// Возвращает бронирование по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор бронирования.</param>
    /// <returns>Информация о бронировании.</returns>
    /// <response code="200">Бронирование найдено.</response>
    /// <response code="404">Бронирование не найдено.</response>
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [HttpGet("{id:guid}")]
    public ActionResult<BookingDto> GetBookingById([FromRoute] Guid id)
    {
        return Ok(_bookingService.GetBookingByIdAsync(id));
    }
}