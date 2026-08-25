using BookingMicroService.Application;
using Common.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingMicroService.Presentation;

/// <summary>
/// Предоставляет HTTP API для управления бронированиями событий.
/// </summary>
/// <remarks>
/// Контроллер реализует операции для создания, получения одного и списка бронирований.
/// </remarks>
/// <param name="bookingService">Сервис, реализующий бизнес-логику для операций с бронированиями.</param>
[ApiController]
[Route("bookings")]
public class BookingsController(IBookingService bookingService) : UserInteractingControllerBase
{
    private readonly IBookingService _bookingService = bookingService;

    /// <summary>
    /// Размещает бронирование для события по идентификатору.
    /// </summary>
    /// <param name="eventId">Идентификатор события, для которого создаётся бронирование.</param>
    /// <returns>Информация о созданном бронировании.</returns>
    /// <response code="202">Бронирование принято к обработке.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Событие не найдено.</response>
    /// <response code="409">Нет доступных мест.</response>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Produces("application/json")]
    [Route("~/events/{eventId:guid}/book")]
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Book([FromRoute] Guid eventId)
    {
        var userId = GetUserIdFromClaims();
        var createdBooking = await _bookingService.CreateAsync(eventId, userId);
        return AcceptedAtAction(nameof(GetBookingById), new { id = createdBooking.Id }, createdBooking);
    }

    /// <summary>
    /// Возвращает бронирование по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор бронирования.</param>
    /// <returns>Информация о бронировании.</returns>
    /// <response code="200">Бронирование найдено.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Бронирование не найдено.</response>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetBookingById([FromRoute] Guid id)
    {
        return Ok(await _bookingService.GetByIdAsync(id));
    }

    /// <summary>
    /// Возвращает список всех бронирований.
    /// </summary>
    /// <response code="200">Список успешно возвращён.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="403">Пользователь не имеет прав доступа.</response>
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpGet]
    public async Task<ActionResult<List<BookingDto>>> GetAllBookings()
    {
        return Ok(await _bookingService.GetAllBookingsAsync());
    }

    /// <summary>
    /// Отменяет бронирование по идентификатору.
    /// </summary>
    /// <param name="bookingId">Идентификатор бронирования.</param>
    /// <returns>Обновленное событие.</returns>
    /// <response code="202">Принята заявка на отмену бронирования.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="403">Пользователь не имеет прав доступа.</response>
    /// <response code="404">Бронирование не найдено.</response>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{bookingId:guid}")]
    public async Task<ActionResult<BookingDto>> CancelBookingById([FromRoute] Guid bookingId)
    {
        var userId = GetUserIdFromClaims();

        await _bookingService.CancelByIdAsync(bookingId, userId);
        return AcceptedAtAction(nameof(GetBookingById), new { bookingId });
    }
}