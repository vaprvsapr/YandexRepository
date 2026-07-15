using EventManager.Domain.Models;
using EventManager.Application.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.Services;

/// <summary>
/// Фоновый сервис для автоматической обработки бронирований.
/// </summary>
/// <remarks>
/// Периодически проверяет наличие бронирований со статусом Pending и подтверждает их через заданный интервал времени.
/// </remarks>
/// <param name="serviceScopeFactory">Фабрика для создания областей видимости сервисов.</param>
/// <param name="logger">Логгер для записи информации о процессе обработки бронирований.</param>
/// <param name="delay">Задержка между проверками бронирований.</param>
/// <param name="maxConcurrentBookings">Максимальное количество бронирований, обрабатываемых одновременно.</param>
public class BookingProcessingService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<BookingProcessingService> logger) : BackgroundService
{
    private readonly ILogger<BookingProcessingService> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private const int _delay = 5000;
    private const int _maxConcurrentBookings = 4;


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
            var scope = _serviceScopeFactory.CreateScope();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

            List<Booking> pendingBookings = await bookingRepository
                .GetAll()
                .Where(b => b.Status == BookingStatus.Pending)
                .OrderBy(b => b.CreatedAt)
                .Take(_maxConcurrentBookings)
                .ToListAsync(stoppingToken);

            if (pendingBookings.Count > 0)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Обработка {n} ожидающих бронирований в: {time}",
                        pendingBookings.Count, DateTime.Now);

                var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking.Id, stoppingToken));
                await Task.WhenAll(tasks);
            }
            else
                await Task.Delay(_delay, stoppingToken);
        }

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("BookingProcessingService остановлен: {time}", DateTime.Now);
    }

    /// <summary>
    /// Метод обработки бронирования.
    /// </summary>
    /// <param name="bookingId">Id бронирования, которое нужно обработать.</param>
    /// <param name="ct">Токен отмены.</param>
    public async Task ProcessBookingAsync(Guid bookingId, CancellationToken ct)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        Booking? bookingToProcess = await bookingRepository.GetByIdAsync(bookingId, ct);
        Event? existingEvent = await eventRepository.GetByIdAsync(bookingToProcess?.EventId ?? Guid.Empty, ct);

        // Упростил логику обработки: если событие существует, подтверждаем бронирование, иначе отклоняем его.
        // Считаю, что на данный момент нет смысла усложнять.
        if (existingEvent != null)
        {
            await bookingRepository.ConfirmByIdAsync(bookingId, ct);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Бронирование {id} для события {title} подтверждено.",
                    bookingToProcess?.Id, existingEvent.Title);
        }
        else
        {
            await bookingRepository.RejectByIdAsync(bookingId, ct);
            _logger.LogWarning("Бронирование с ID {id} отклонено. Событие с ID {eventId} не найдено.",
                bookingToProcess?.Id, bookingToProcess?.EventId);
        }
    }
}