using EventManager.Interfaces;
using EventManager.Models.Bookings;

namespace EventManager.Services;

public class BookingService(IRepository<Booking> bookingRepository) : IBookingService
{
    private readonly IRepository<Booking> _bookingRepository = bookingRepository;

    // Пересмотреть урок, где было похожее. Нужно ли писать async, await?
    public void CreateBookingAsync(BookingDto bookingDto)
    {
        Booking newBooking = new Booking
        {
            Id = 0, // This should be set by the database when the booking is created
            EventId = bookingDto.EventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _bookingRepository.Add(newBooking);
    }

    public Booking? GetBookingByIdAsync(int id)
    {
        return _bookingRepository.GetById(id);
    }
}
