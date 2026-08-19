using BookingMicroService.Domain;
using Contracts;

namespace BookingMicroService.Application;

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
            UserId = booking.UserId,
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
            UserId = bookingDto.UserId,
            Status = bookingDto.Status,
            CreatedAt = bookingDto.CreatedAt,
            ProcessedAt = bookingDto.ProcessedAt
        };
    }

    /// <summary>
    /// Метод преобразования модели бронирования в событие создания бронирования.
    /// </summary>
    /// <param name="booking"></param>
    /// <returns></returns>
    public static BookingCreated ToBookingCreated(Booking booking)
    {
        return new BookingCreated
        {
            BookingId = booking.Id,
            UserId = booking.UserId,
            EventId = booking.EventId
        };
    }
}
