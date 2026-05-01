namespace EventManager.Models.Bookings;

public class BookingMapper
{
    public static BookingDto ToBookingDto(Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt
        };
    }

    public static Booking ToBooking(BookingDto bookingDto)
    {
        return new Booking
        {
            Id = bookingDto.Id,
            EventId = bookingDto.EventId,
            Status = bookingDto.Status,
            CreatedAt = bookingDto.CreatedAt,
            ProcessedAt = bookingDto.ProcessedAt
        };
    }
}
