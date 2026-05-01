using EventManager.Interfaces;
using EventManager.Models.Bookings;

namespace EventManager.Services;

/// <summary>
/// Фоновый сервис для автоматической обработки бронирований.
/// </summary>
/// <remarks>
/// Периодически проверяет наличие бронирований со статусом Pending и подтверждает их через заданный интервал времени.
/// </remarks>
/// <param name="bookingRepository">Репозиторий для работы с бронированиями.</param>
public class BookingProcessingService(IRepository<Booking> bookingRepository) : BackgroundService
{
    private readonly IRepository<Booking> _bookingRepository = bookingRepository;
    /// <summary>
    /// Запускает фоновую задачу обработки бронирований.
    /// </summary>
    /// <param name="stoppingToken">Токен отмены для корректного завершения работы сервиса.</param>
    /// <returns>Задача, представляющая выполнение фоновой обработки.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Booking? pendingBooking = _bookingRepository
                .GetAll()
                .FirstOrDefault(b => b.Status is BookingStatus.Pending);
            if (pendingBooking is not null)
            {
                await Task.Delay(5000, stoppingToken); // Симуляция обработки брони
                pendingBooking.Status = BookingStatus.Confirmed;
            }
            else
                await Task.Delay(1000, stoppingToken);
        }
    }
}
