namespace EventManager.Models.Bookings;

/// <summary>
/// Маппер для преобразования между моделями бронирования и их DTO.
/// </summary>
public class BookingMapper
{
    /// <summary>
    /// Метод преобразования модели бронирования в DTO.
    /// </summary>
    /// <param name="booking"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Метод преобразования DTO бронирования обратно в модель.
    /// </summary>
    /// <param name="bookingDto"></param>
    /// <returns></returns>
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
