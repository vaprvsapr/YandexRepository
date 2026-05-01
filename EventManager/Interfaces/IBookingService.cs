using EventManager.Models.Bookings;

namespace EventManager.Interfaces;

public interface IBookingService
{
    public BookingDto CreateBookingAsync(BookingDto bookingDto);
    public BookingDto? GetBookingByIdAsync(Guid id);
}
