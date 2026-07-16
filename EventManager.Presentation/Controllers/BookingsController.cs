using EventManager.Application.Dto;
using EventManager.Application.Services.Interfaces;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;


namespace EventManager.Presentation.Controllers;

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
    /// <param name="eventId">Идентификатор события, для которого создаётся бронирование.</param>
    /// <returns>Информация о созданном бронировании.</returns>
    /// <response code="202">Бронирование принято к обработке.</response>
    /// <response code="404">Событие не найдено.</response>
    /// <response code="409">Нет доступных мест.</response>
    [Authorize]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [Produces("application/json")]
    [Route("~/events/{eventId:guid}/book")]
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Book([FromRoute] Guid eventId)
    {
        Guid userId = GetUserIdFromClaims();
        BookingDto createdBooking = await _bookingService.CreateBookingAsync(eventId, userId);
        return Accepted($"/bookings/{createdBooking.Id}", createdBooking);
    }

    /// <summary>
    /// Возвращает список бронирований для события по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns>Список бронирований для события.</returns>
    /// <response code="200">Список успешно возвращён.</response>
    /// <response code="404">Событие не найдено.</response>
    [Authorize]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [HttpGet("event/{id:guid}")]
    public async Task<ActionResult<List<BookingDto>>> GetBookingsByEventId([FromRoute] Guid id)
    {
        return Ok((await _bookingService.GetBookingsByEventIdAsync(id))
            .Where(b => b.UserId == GetUserIdFromClaims()));
    }

    /// <summary>
    /// Возвращает бронирование по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор бронирования.</param>
    /// <returns>Информация о бронировании.</returns>
    /// <response code="200">Бронирование найдено.</response>
    /// <response code="404">Бронирование не найдено.</response>
    [Authorize]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetBookingById([FromRoute] Guid id)
    {
        var existingBooking = await _bookingService.GetBookingByIdAsync(id);
        if (existingBooking?.UserId != GetUserIdFromClaims())
            return Forbid();
        return Ok(await _bookingService.GetBookingByIdAsync(id));
    }

    /// <summary>
    /// Возвращает список всех бронирований.
    /// </summary>
    /// <response code="200">Список успешно возвращён.</response>
    [Authorize(Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [HttpGet]
    public async Task<ActionResult<List<BookingDto>>> GetAllBookings()
    {
        return Ok(await _bookingService.GetAllBookingsAsync());
    }

    /// <summary>
    /// Удаляет бронирование по идентификатору.
    /// </summary>
    /// <param name="id"></param>
    [Authorize(Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBookingById([FromRoute] Guid id)
    {
        await _bookingService.DeleteBookingByIdAsync(id);
        return NoContent();
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelBookingById([FromRoute] Guid id)
    {
        var existingBooking = await _bookingService.GetBookingByIdAsync(id);
        if (existingBooking?.UserId != GetUserIdFromClaims() || !GetUserRoleFromClaims().Equals(UserRole.Admin))
            return Forbid();
        await _bookingService.CancelBookingByIdAsync(id);
        return Ok();
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
        return userId;
    }
    private UserRole GetUserRoleFromClaims()
    {
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role");
        if (roleClaim == null)
            throw new InvalidOperationException("Роль пользователя не найдена в токене.");
        return Enum.Parse<UserRole>(roleClaim.Value);
    }
}