using EventManager.Application.Dto;
using EventManager.Application.Services.Interfaces;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        Guid userId = GetUserIdFromClaims();
        BookingDto createdBooking = await _bookingService.CreateBookingAsync(eventId, userId);
        return Accepted($"/bookings/{createdBooking.Id}", createdBooking);
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
        var existingBooking = await _bookingService.GetBookingByIdAsync(id);
        if (existingBooking?.UserId != GetUserIdFromClaims())
            return Forbid();
        return Ok(await _bookingService.GetBookingByIdAsync(id));
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
    /// <param name="id">Идентификатор бронирования.</param>
    /// <returns>Обновленное событие.</returns>
    /// <response code="204">Бронирование успешно отменено.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="403">Пользователь не имеет прав доступа.</response>
    /// <response code="404">Бронирование не найдено.</response>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<BookingDto>> CancelBookingById([FromRoute] Guid id)
    {
        var existingBooking = await _bookingService.GetBookingByIdAsync(id);
        if (existingBooking?.UserId != GetUserIdFromClaims() && !GetUserRoleFromClaims().Equals(UserRole.Admin))
            return Forbid();
        return NoContent();
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
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role") ??
            throw new InvalidOperationException("Роль пользователя не найдена в токене.");
        return Enum.Parse<UserRole>(roleClaim.Value);
    }
}