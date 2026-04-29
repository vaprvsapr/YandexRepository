using EventManager.Models.Bookings;

namespace EventManager.Interfaces;

public interface IBookingService
{
    public void CreateBookingAsync(BookingDto bookingDto);
    public Booking? GetBookingByIdAsync(Guid id);
}
