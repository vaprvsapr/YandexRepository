using Contracts;
using EventMicroService.Domain;
using EventMicroService.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace EventMicroService.Infrastructure;

public class BookingProcessingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly BookingProcessResponseProducer _producer;
    private readonly ILogger<BookingProcessingBackgroundService> _logger;
    private readonly IConsumer<string, string> _consumer;

    public BookingProcessingBackgroundService(
        IServiceScopeFactory serviceScopeFactory, 
        BookingProcessResponseProducer producer, 
        ILogger<BookingProcessingBackgroundService> logger, 
        string bootstrapServers, string groupId)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _producer = producer;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            //EnableAutoOffsetStore = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(Topics.BookingProcessRequest);
    }

    /// <summary>
    /// Запускает фоновую задачу обработки бронирований.
    /// </summary>
    /// <param name="stoppingToken">Токен отмены для корректного завершения работы сервиса.</param>
    /// <returns>Задача, представляющая выполнение фоновой обработки.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("BookingConfirmationBackgroundService started at: {time}", DateTime.Now);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //var scope = _serviceScopeFactory.CreateScope();
                //var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var consumeResult = _consumer.Consume(stoppingToken);

                var bookingProcessRequest = System.Text.Json.JsonSerializer.Deserialize<BookingProcessRequest>(consumeResult.Message.Value);

                if (bookingProcessRequest == null)
                {
                    _logger.LogWarning("Получено пустое сообщение бронирования из Kafka.");
                    continue;
                }

                var scope = _serviceScopeFactory.CreateScope();
                var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var eventBookingRepository = scope.ServiceProvider.GetRequiredService<IEventBookingRepository>();
                var existingEvent = await eventRepository.GetByIdAsync(bookingProcessRequest.EventId, stoppingToken);

                var bookingProcessResponse = new BookingProcessResponse
                {
                    BookingId = bookingProcessRequest.BookingId,
                    Result = BookingProcessResult.Failed
                };

                if (existingEvent == null)
                {
                    _logger.LogWarning("Событие с ID {eventId} не найдено для бронирования с ID {bookingId}.", bookingProcessRequest.EventId, bookingProcessRequest.EventId);
                    await _producer.PublishAsync(bookingProcessResponse);
                    continue;
                }

                if (bookingProcessRequest.Command == BookingProcessCommand.Cancel)
                {
                    existingEvent.ReleaseSeats();
                    await eventRepository.UpdateAsync(existingEvent, stoppingToken);

                    var existingBooking = await eventBookingRepository.GetByBookingIdAsync(bookingProcessRequest.BookingId, stoppingToken);
                    if (existingBooking != null)
                        await eventBookingRepository.DeleteAsync(existingBooking, stoppingToken);

                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("Бронирование с ID {bookingId} отменено для события с ID {eventId}.", bookingProcessRequest.BookingId, bookingProcessRequest.EventId);
                    bookingProcessResponse.Result = BookingProcessResult.Cancelled;
                    await _producer.PublishAsync(bookingProcessResponse);
                    continue;
                }

                if (existingEvent.AvailableSeats == 0)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("Нет доступных мест для события с ID {eventId} для бронирования с ID {bookingId}.", bookingProcessRequest.EventId, bookingProcessRequest.EventId);
                    await _producer.PublishAsync(bookingProcessResponse);
                    continue;
                }

                if (existingEvent.StartAt <= DateTime.UtcNow)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("Событие с ID {eventId} уже началось для бронирования с ID {bookingId}.", bookingProcessRequest.EventId, bookingProcessRequest.EventId);
                    await _producer.PublishAsync(bookingProcessResponse);
                    continue;
                }

                existingEvent.TryReserveSeats();
                await eventRepository.UpdateAsync(existingEvent, stoppingToken);

                var eventBooking = new EventBooking { EventId = bookingProcessRequest.EventId, BookingId = bookingProcessRequest.BookingId };
                await eventBookingRepository.CreateAsync(eventBooking, stoppingToken);

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Бронирование с ID {bookingId} подтверждено для события с ID {eventId}.", bookingProcessRequest.BookingId, bookingProcessRequest.EventId);
                bookingProcessResponse.Result = BookingProcessResult.Confirmed;
                await _producer.PublishAsync(bookingProcessResponse);
            }
        }
        catch (ConsumeException ex)
        {
            _logger.LogError(ex, "Ошибка при потреблении сообщения из Kafka: {message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Произошла ошибка в BookingConfirmationBackgroundService: {message}", ex.Message);
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
        }


        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("BookingConfirmationBackgroundService остановлен: {time}", DateTime.Now);
    }
}