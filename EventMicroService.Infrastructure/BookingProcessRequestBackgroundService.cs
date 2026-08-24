using Common.Contracts;
using EventMicroService.Domain;
using EventMicroService.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace EventMicroService.Infrastructure;

public class BookingProcessRequestBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly BookingProcessResponseProducer _producer;
    private readonly ILogger<BookingProcessRequestBackgroundService> _logger;
    private readonly IConsumer<string, string> _consumer;

    public BookingProcessRequestBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        BookingProcessResponseProducer producer,
        ILogger<BookingProcessRequestBackgroundService> logger,
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
            EnableAutoOffsetStore = false
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


        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                // Считываем сообщение из Kafka и десериализуем его в объект BookingProcessRequest
                BookingProcessRequest? bookingProcessRequest;
                try
                {
                    bookingProcessRequest = System.Text.Json.JsonSerializer.Deserialize<BookingProcessRequest>(consumeResult.Message.Value);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    _logger.LogError(ex, "Ошибка десериализации сообщения бронирования из Kafka: {message}", ex.Message);
                    _consumer.Commit(consumeResult);
                    continue;
                }

                if (bookingProcessRequest == null)
                {
                    _logger.LogWarning("Получено пустое сообщение бронирования из Kafka.");
                    _consumer.Commit(consumeResult);
                    continue;
                }

                // Проводим проверки и обработку бронирования в рамках отдельного скоупа зависимостей
                using var scope = _serviceScopeFactory.CreateScope();
                var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var eventBookingRepository = scope.ServiceProvider.GetRequiredService<IEventBookingRepository>();
                var existingEvent = await eventRepository.GetByIdAsync(bookingProcessRequest.EventId, stoppingToken);

                var bookingProcessResponse = new BookingProcessResponse
                {
                    BookingId = bookingProcessRequest.BookingId,
                    Result = BookingProcessResult.Error
                };

                if (existingEvent == null)
                {
                    _logger.LogWarning("Событие с ID {eventId} не найдено для бронирования с ID {bookingId}.", bookingProcessRequest.EventId, bookingProcessRequest.EventId);
                    await _producer.PublishAsync(bookingProcessResponse);

                    _consumer.Commit(consumeResult);
                    continue;
                }

                if (bookingProcessRequest.Command == BookingProcessCommand.Cancel)
                {
                    var existingBooking = await eventBookingRepository.GetByBookingIdAsync(bookingProcessRequest.BookingId, stoppingToken);
                    if (existingBooking != null)
                    {
                        await eventBookingRepository.DeleteAsync(existingBooking, stoppingToken);
                        existingEvent.ReleaseSeats();
                        await eventRepository.UpdateAsync(existingEvent, stoppingToken);

                        bookingProcessResponse.Result = BookingProcessResult.Cancelled;
                        await _producer.PublishAsync(bookingProcessResponse);

                        if (_logger.IsEnabled(LogLevel.Information))
                            _logger.LogInformation("Бронирование с ID {bookingId} отменено для события с ID {eventId}.",
                                bookingProcessRequest.BookingId, bookingProcessRequest.EventId);
                    }
                    else
                    {
                        await _producer.PublishAsync(bookingProcessResponse);

                        if (_logger.IsEnabled(LogLevel.Information))
                            _logger.LogInformation("Бронирование с ID {bookingId} не найдено для отмены для события с ID {eventId}.",
                                bookingProcessRequest.BookingId, bookingProcessRequest.EventId);
                    }
                    _consumer.Commit(consumeResult);
                    continue;
                }

                if (existingEvent.AvailableSeats == 0)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("Нет доступных мест для события с ID {eventId} для бронирования с ID {bookingId}.", bookingProcessRequest.EventId, bookingProcessRequest.EventId);
                    await _producer.PublishAsync(bookingProcessResponse);

                    _consumer.Commit(consumeResult);
                    continue;
                }

                if (existingEvent.StartAt <= DateTime.UtcNow)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("Событие с ID {eventId} уже началось для бронирования с ID {bookingId}.", bookingProcessRequest.EventId, bookingProcessRequest.EventId);
                    await _producer.PublishAsync(bookingProcessResponse);

                    _consumer.Commit(consumeResult);
                    continue;
                }

                // Проверяем, существует ли уже бронирование для данного события и идентификатора бронирования
                var eventBooking = await eventBookingRepository.GetEventBookingsAsync(
                    bookingProcessRequest.EventId, bookingProcessRequest.BookingId, stoppingToken);
                // Если бронирование уже существует, пропускаем его
                if (eventBooking == null)
                {
                    eventBooking = new EventBooking
                    {
                        EventId = bookingProcessRequest.EventId,
                        BookingId = bookingProcessRequest.BookingId
                    };
                    // Если бронирование не существует, добавляем его в таблицу для отслеживания
                    await eventBookingRepository.CreateAsync(eventBooking, stoppingToken);

                    existingEvent.TryReserveSeats();
                    await eventRepository.UpdateAsync(existingEvent, stoppingToken);

                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("Бронирование с ID {bookingId} подтверждено для события с ID {eventId}.", bookingProcessRequest.BookingId, bookingProcessRequest.EventId);
                    bookingProcessResponse.Result = BookingProcessResult.Confirmed;
                    await _producer.PublishAsync(bookingProcessResponse);
                }
                _consumer.Commit(consumeResult);

            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Ошибка при потреблении сообщения из Kafka: {message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка в BookingConfirmationBackgroundService: {message}", ex.Message);
            }
        }

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("BookingConfirmationBackgroundService остановлен: {time}", DateTime.Now);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Close();
        _consumer.Dispose();
        await base.StopAsync(cancellationToken);
    }
}