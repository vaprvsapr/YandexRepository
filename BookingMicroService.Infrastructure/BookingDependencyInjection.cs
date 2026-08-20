using BookingMicroService.Application;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingMicroService.Infrastructure;

/// <summary>
/// Методы расширения для регистрации зависимостей в контейнере внедрения зависимостей.
/// </summary>
public static partial class DependencyInjectionExtensions
{
    /// <summary>
    /// Добавляет инфраструктурные сервисы в коллекцию служб приложения.
    /// </summary>
    public static IServiceCollection AddBookingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BookingDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("BookingDbConnection"));
        });

        // Сервис бронирования и его репозиторий
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingService, BookingService>();

        services.AddSingleton<IBookingCreatedProducer>(sp =>
        {
            var config = configuration.GetSection("KafkaSettings");
            var bootstrapServers = config.GetValue<string>("BootstrapServers") ??
                throw new InvalidOperationException("BootstrapServers configuration is missing.");
            return new BookingCreatedProducer(bootstrapServers);

        });

        // Фоновый сервис для обработки бронирований
        services.AddSingleton<IHostedService>(sp =>
        {
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var logger = sp.GetRequiredService<ILogger<BookingConfirmationResponseBackgroundService>>();

            var config = configuration.GetSection("KafkaSettings");
            var groupId = config.GetRequiredSection("BookingConfirmationResponseGroupId").Value
                ?? throw new InvalidOperationException("GroupId configuration is missing.");
            var bootstrapServers = config.GetValue<string>("BootstrapServers")
                ?? throw new InvalidOperationException("BootstrapServers configuration is missing.");

            return new BookingConfirmationResponseBackgroundService(
                scopeFactory,
                logger,
                bootstrapServers,
                groupId
            );
        });

        return services;
    }
}

