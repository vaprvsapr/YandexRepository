using EventManager.Data;
using EventManager.Interfaces;
using EventManager.Models.Bookings;
using EventManager.Models.Events;

namespace EventManager.Services;

public class BookingService
    (IRepository<Booking> bookingRepository, IRepository<Event> eventRepository) : IBookingService
{
    private readonly IRepository<Booking> _bookingRepository = bookingRepository;
    private readonly IRepository<Event> _eventRepository = eventRepository;

    // Пересмотреть урок, где было похожее. Нужно ли писать async, await?
    public BookingDto CreateBookingAsync(BookingDto bookingDto)
    {
        // Проверка, что указанное событие существует.
        if (_eventRepository.GetById(bookingDto.EventId) is null)
            throw new KeyNotFoundException($"Событие с Id:{bookingDto.EventId} не найдено.");

        Booking newBooking = new Booking
        {
            Id = Guid.NewGuid(), // Создаем новое Id для брони.
            EventId = bookingDto.EventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _bookingRepository.Add(newBooking);

        return new BookingDto() 
        { 
            EventId = bookingDto.EventId, 
            Id = newBooking.Id, 
            Status = newBooking.Status
        };
    }

    public BookingDto? GetBookingByIdAsync(Guid id)
    {
        var existingBooking = _bookingRepository.GetById(id);
        if (existingBooking is null)
            throw new KeyNotFoundException($"Бронь с Id:{id} не найдена.");
        return new BookingDto() 
        { 
            EventId = existingBooking.EventId, 
            Id = existingBooking.Id, 
            Status = existingBooking.Status 
        };
    }
}
