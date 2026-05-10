using EventManager.Interfaces;
using EventManager.Models.Bookings;
using EventManager.Models.Events;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace EventManager.Services;

/// <summary>
/// Фоновый сервис для автоматической обработки бронирований.
/// </summary>
/// <remarks>
/// Периодически проверяет наличие бронирований со статусом Pending и подтверждает их через заданный интервал времени.
/// </remarks>
/// <param name="bookingRepository">Репозиторий для работы с бронированиями.</param>
/// <param name="eventRepository">Репозиторий для работы с событиями.</param>
/// <param name="logger">Логгер для записи информации о процессе обработки бронирований.</param>
/// <param name="delay">Задержка между проверками бронирований.</param>
public class BookingProcessingService(
    IRepository<Booking> bookingRepository,
    IRepository<Event> eventRepository,
    ILogger<BookingProcessingService> logger,
    int delay = 1000) : BackgroundService
{
    private readonly IRepository<Booking> _bookingRepository = bookingRepository;
    private readonly IRepository<Event> _eventRepository = eventRepository;
    private readonly ILogger<BookingProcessingService> _logger = logger;
    private readonly int _delay = delay;

    /// <summary>
    /// Запускает фоновую задачу обработки бронирований.
    /// </summary>
    /// <param name="stoppingToken">Токен отмены для корректного завершения работы сервиса.</param>
    /// <returns>Задача, представляющая выполнение фоновой обработки.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("BookingProcessingService started at: {time}", DateTime.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            Booking? pendingBooking = _bookingRepository
                .GetAll()
                .FirstOrDefault(b => b.Status is BookingStatus.Pending);
            if (pendingBooking is not null)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Processing pending booking with ID: {bookingId} at: {time}", pendingBooking.Id, DateTime.Now);

                await ProcessBooking(pendingBooking, _delay, stoppingToken);
            }
            else
                await Task.Delay(_delay, stoppingToken);
        }

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("BookingProcessingService stopped at: {time}", DateTime.Now);
    }

    /// <summary>
    /// Метод обработки бронирования.
    /// </summary>
    /// <param name="bookingToProcess">Бронировани, которое нужно обработать.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <param name="delay">Время обработки.</param>
    public async Task ProcessBooking(Booking bookingToProcess, int delay = 5000, CancellationToken ct = default)
    {
        await Task.Delay(delay, ct); // Симуляция обработки
        int? numberOfSeats = _eventRepository.GetById(bookingToProcess.EventId)?.NumberOfSeats;
        int numberOfBookings = _bookingRepository
            .GetAll()
            .Count(
            b => b.EventId == bookingToProcess.EventId &&
            b.Status == BookingStatus.Confirmed
            ); // Количество подтвержденных бронирований

        bookingToProcess.Status = numberOfSeats is null || (int)numberOfSeats > numberOfBookings ?
            BookingStatus.Confirmed :
            BookingStatus.Rejected;
        bookingToProcess.ProcessedAt = DateTime.Now;
    }
}



