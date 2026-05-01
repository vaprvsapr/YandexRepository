using EventManager.Interfaces;
using EventManager.Models.Bookings;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventManager.Controllers;

[ApiController]
[Route("bookings")]
public class BookingsController
    (IBookingService bookingService) : ControllerBase
{
    private readonly IBookingService _bookingService = bookingService;

    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [Route("~/events/book/{id}/book")] // "~/" - игнорирует маршрут контроллера.
    [HttpPost] // Почему же не bookings/...
    public ActionResult<BookingDto> Book([FromRoute] Guid id)
    {
        BookingDto newBookingDto = new BookingDto() { EventId = id};
        BookingDto createdBooking = _bookingService.CreateBookingAsync(newBookingDto);
        return Accepted($"/bookings/{createdBooking.Id}", createdBooking);
    }

    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [HttpGet("bookings/{id:guid}")]
    public ActionResult<List<BookingDto>> GetBookingById([FromRoute] Guid id)
    {
        return Ok(_bookingService.GetBookingByIdAsync(id));
    }
}
