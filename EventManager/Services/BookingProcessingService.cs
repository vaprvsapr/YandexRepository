using EventManager.Interfaces;
using EventManager.Models.Bookings;
using EventManager.Models.Events;
using Microsoft.AspNetCore.Http.HttpResults;
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
    int delay = 5000) : BackgroundService
{
    private readonly IRepository<Booking> _bookingRepository = bookingRepository;
    private readonly IRepository<Event> _eventRepository = eventRepository;
    private readonly ILogger<BookingProcessingService> _logger = logger;
    private readonly int _delay = delay;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

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
            List<Booking> pendingBookings = [.. _bookingRepository
                .GetAll()
                .Where(b => b.Status is BookingStatus.Pending)
                .Take(4)];

            if (pendingBookings.Count > 0)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Processing {n} pending bookings at: {time}", 
                        pendingBookings.Count, DateTime.Now);

                var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, _delay, stoppingToken));
                await Task.WhenAll(tasks);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("No available pending bookings at: {time}", DateTime.Now);
                await Task.Delay(_delay, stoppingToken);
            }
        }

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("BookingProcessingService stopped at: {time}", DateTime.Now);
    }

    /// <summary>
    /// Метод обработки бронирования.
    /// </summary>
    /// <param name="bookingToProcess">Бронировани, которое нужно обработать.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <param name="delay">Задержка перед обработкой.</param>
    public async Task ProcessBookingAsync(Booking bookingToProcess, int delay, CancellationToken ct)
    {
        await Task.Delay(delay, ct); // Симуляция обработки
        Event? existingEvent = _eventRepository.GetById(bookingToProcess.EventId);
        try
        {
            await _semaphore.WaitAsync(ct);
            if (existingEvent is not null)
            {
                bookingToProcess.Confirm();
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Booking {id} for event {title} confirmed.", 
                        bookingToProcess.Id, existingEvent.Title);
            }
            else
            {
                bookingToProcess.Reject();
                _logger.LogWarning("Booking {id} rejected due to missing event with ID {eventId}.",
                    bookingToProcess.Id, bookingToProcess.EventId);
            }
        }
        catch
        {
            bookingToProcess.Reject();
            existingEvent?.ReleaseSeats();
            if (existingEvent != null)
                _logger.LogWarning("Booking {id} rejected due to an error during processing. Seats released for event {eventId}.", 
                    bookingToProcess.Id, bookingToProcess.EventId);
            else
                _logger.LogWarning("Booking {id} rejected due to an error during processing. Event with ID {eventId} not found.",
                    bookingToProcess.Id, bookingToProcess.EventId);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}



