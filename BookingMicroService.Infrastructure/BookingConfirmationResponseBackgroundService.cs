using Contracts;
using BookingMicroService.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace BookingMicroService.Infrastructure;

public class BookingConfirmationResponseBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BookingConfirmationResponseBackgroundService> _logger;
    private readonly IConsumer<string, string> _consumer;

    public BookingConfirmationResponseBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BookingConfirmationResponseBackgroundService> logger,
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
            //EnableAutoOffsetStore = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(Topics.BookingCreatedResponseTopic);
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

                var bookingCreatedResponse = System.Text.Json.JsonSerializer.Deserialize<BookingCreatedResponse>(consumeResult.Message.Value);

                if (bookingCreatedResponse == null)
                {
                    _logger.LogWarning("Получено пустое сообщение бронирования из Kafka.");
                    continue;
                }

                var booking = await bookingRepository.GetByIdAsync(bookingCreatedResponse.BookingId, stoppingToken) ??
                    throw new KeyNotFoundException($"Бронирование с ID {bookingCreatedResponse.BookingId} не найдено в репозитории.");

                if (bookingCreatedResponse.Confirmed)
                {
                    await bookingRepository.ConfirmAsync(booking, stoppingToken);
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("Бронирование с ID {bookingId} подтверждено.", bookingCreatedResponse.BookingId);
                }
                else
                {
                    await bookingRepository.RejectAsync(booking, stoppingToken);
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("Бронирование с ID {bookingId} отклонено.", bookingCreatedResponse.BookingId);
                }
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