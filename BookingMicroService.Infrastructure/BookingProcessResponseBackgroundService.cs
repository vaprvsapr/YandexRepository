using Contracts;
using BookingMicroService.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace BookingMicroService.Infrastructure;

public class BookingProcessResponseBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BookingProcessResponseBackgroundService> _logger;
    private readonly IConsumer<string, string> _consumer;

    public BookingProcessResponseBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BookingProcessResponseBackgroundService> logger,
        string bootstrapServers, string groupId)
    {
        _serviceScopeFactory = serviceScopeFactory;
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
        _consumer.Subscribe(Topics.BookingProcessResponse);
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
                var scope = _serviceScopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var consumeResult = _consumer.Consume(stoppingToken);

                var bookingCreatedResponse = System.Text.Json.JsonSerializer.Deserialize<BookingProcessResponse>(consumeResult.Message.Value);

                if (bookingCreatedResponse == null)
                {
                    _logger.LogWarning("Получено пустое сообщение бронирования из Kafka.");

                    _consumer.Commit(consumeResult);
                    continue;
                }

                var booking = await bookingRepository.GetByIdAsync(bookingCreatedResponse.BookingId, stoppingToken) ??
                    throw new KeyNotFoundException($"Бронирование с ID {bookingCreatedResponse.BookingId} не найдено в репозитории.");

                if (bookingCreatedResponse.Result == BookingProcessResult.Confirmed)
                {
                    await bookingRepository.ConfirmAsync(booking, stoppingToken);
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("Бронирование с ID {bookingId} подтверждено.", bookingCreatedResponse.BookingId);

                    _consumer.Commit(consumeResult);
                    continue;
                }

                if (bookingCreatedResponse.Result == BookingProcessResult.Cancelled)
                {
                    await bookingRepository.CancelAsync(booking, stoppingToken);
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("Бронирование с ID {bookingId} отменено.", bookingCreatedResponse.BookingId);

                    _consumer.Commit(consumeResult);
                    continue;
                }

                await bookingRepository.RejectAsync(booking, stoppingToken);
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Бронирование с ID {bookingId} отклонено.", bookingCreatedResponse.BookingId);
                _consumer.Commit(consumeResult);
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