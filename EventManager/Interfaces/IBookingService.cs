using EventManager.Models.Bookings;

namespace EventManager.Interfaces;

public interface IBookingService
{
    public BookingDto CreateBookingAsync(Guid eventId);
    public BookingDto? GetBookingByIdAsync(Guid id);
    public List<BookingDto> GetBookingsByEventIdAsync(Guid eventId);
}
