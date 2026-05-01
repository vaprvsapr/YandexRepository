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
    public BookingDto CreateBookingAsync(Guid eventId)
    {
        // Проверка, что указанное событие существует.
        if (_eventRepository.GetById(eventId) is null)
            throw new KeyNotFoundException($"Событие с Id:{eventId} не найдено.");

        Booking newBooking = new Booking
        {
            Id = Guid.NewGuid(), // Создаем новое Id для брони.
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
            ProcessedAt = DateTime.Now
        };
        _bookingRepository.Add(newBooking);

        return BookingMapper.ToBookingDto(newBooking);
    }

    public BookingDto? GetBookingByIdAsync(Guid id)
    {
        var existingBooking = _bookingRepository.GetById(id);
        if (existingBooking is null)
            throw new KeyNotFoundException($"Бронь с Id:{id} не найдена.");
        return BookingMapper.ToBookingDto(existingBooking);
    }

    public List<BookingDto> GetBookingsByEventIdAsync(Guid eventId)
    {
        if(_eventRepository.GetById(eventId) is null)
            throw new KeyNotFoundException($"Событие с Id:{eventId} не найдено.");
        return _bookingRepository
            .GetAll()
            .Where(b => b.EventId == eventId)
            .Select(BookingMapper.ToBookingDto).ToList();
    }
}
