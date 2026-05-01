using EventManager.Interfaces;
using EventManager.Models.Bookings;

namespace EventManager.Services;

public class BookingProcessingService(IRepository<Booking> bookingRepository) : BackgroundService
{
    private readonly IRepository<Booking> _bookingRepository = bookingRepository;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Booking? pendingBooking = _bookingRepository
                .GetAll()
                .FirstOrDefault(b => b.Status is BookingStatus.Pending);
            if (pendingBooking is not null)
            {
                await Task.Delay(5000); // Симуляция обработки брони
                pendingBooking.Status = BookingStatus.Confirmed;
            }
            else
                await Task.Delay(1000);
        }
    }
}
