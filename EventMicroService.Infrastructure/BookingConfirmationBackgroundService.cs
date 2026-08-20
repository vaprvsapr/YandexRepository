using Contracts;
using EventMicroService.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace EventMicroService.Infrastructure;

public class BookingConfirmationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly BookingCreatedResponseProducer _producer;
    private readonly ILogger<BookingConfirmationBackgroundService> _logger;
    private readonly IConsumer<string, string> _consumer;

    public BookingConfirmationBackgroundService(
        IServiceScopeFactory serviceScopeFactory, 
        BookingCreatedResponseProducer producer, 
        ILogger<BookingConfirmationBackgroundService> logger, 
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
        _consumer.Subscribe(Topics.BookingCreatedTopic);
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

                var bookingCreated = System.Text.Json.JsonSerializer.Deserialize<BookingCreated>(consumeResult.Message.Value);

                if (bookingCreated == null)
                {
                    _logger.LogWarning("Получено пустое сообщение бронирования из Kafka.");
                    continue;
                }

                var scope = _serviceScopeFactory.CreateScope();
                var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var existingEvent = await eventRepository.GetByIdAsync(bookingCreated.EventId, stoppingToken);

                var bookingCreatedResponse = new BookingCreatedResponse
                {
                    BookingId = bookingCreated.BookingId,
                    Confirmed = false,
                };

                if (existingEvent == null)
                {
                    _logger.LogWarning("Событие с ID {eventId} не найдено для бронирования с ID {bookingId}.", bookingCreated.EventId, bookingCreated.EventId);
                    await _producer.PublishAsync(bookingCreatedResponse);
                    continue;
                }
                if (existingEvent.AvailableSeats == 0)
                {
                    _logger.LogWarning("Нет доступных мест для события с ID {eventId} для бронирования с ID {bookingId}.", bookingCreated.EventId, bookingCreated.EventId);
                    await _producer.PublishAsync(bookingCreatedResponse);
                    continue;
                }

                existingEvent.TryReserveSeats();
                await eventRepository.UpdateAsync(existingEvent, stoppingToken);
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Бронирование с ID {bookingId} подтверждено для события с ID {eventId}.", bookingCreated.BookingId, bookingCreated.EventId);
                bookingCreatedResponse.Confirmed = true;
                await _producer.PublishAsync(bookingCreatedResponse);
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