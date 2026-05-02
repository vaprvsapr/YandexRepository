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
/// <param name="logger">Логгер для записи информации о процессе обработки бронирований.</param>
public class BookingProcessingService(IRepository<Booking> bookingRepository, ILogger<BookingProcessingService> logger) : BackgroundService
{
    private readonly IRepository<Booking> _bookingRepository = bookingRepository;
    private readonly ILogger<BookingProcessingService> _logger = logger;

    /// <summary>
    /// Запускает фоновую задачу обработки бронирований.
    /// </summary>
    /// <param name="stoppingToken">Токен отмены для корректного завершения работы сервиса.</param>
    /// <returns>Задача, представляющая выполнение фоновой обработки.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingProcessingService started at: {time}", DateTime.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            Booking? pendingBooking = _bookingRepository
                .GetAll()
                .FirstOrDefault(b => b.Status is BookingStatus.Pending);
            if (pendingBooking is not null)
            {
                _logger.LogInformation($"Processing booking with ID: {pendingBooking.Id} at: {DateTime.Now}");
                await Task.Delay(5000, stoppingToken); // Симуляция обработки брони
                pendingBooking.Status = BookingStatus.Confirmed;
                pendingBooking.ProcessedAt = DateTime.Now;
            }
            else
                await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("BookingProcessingService stopped at: {time}", DateTime.Now);
    }
}
