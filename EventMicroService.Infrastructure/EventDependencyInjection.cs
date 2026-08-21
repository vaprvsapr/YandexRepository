using Contracts;
using EventMicroService.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventMicroService.Infrastructure;

/// <summary>
/// Методы расширения для регистрации зависимостей в контейнере внедрения зависимостей.
/// </summary>
public static partial class DependencyInjectionExtensions
{
    /// <summary>
    /// Добавляет инфраструктурные сервисы в коллекцию служб приложения.
    /// </summary>
    public static IServiceCollection AddEventInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("EventDbConnection"));
        });

        // Сервис событий и его репозиторий
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventBookingRepository, EventBookingRepository>();
        services.AddScoped<IEventService, EventService>();

        var config = configuration.GetSection("KafkaSettings");
        var bootstrapServers = config.GetValue<string>("BootstrapServers") ??
            throw new InvalidOperationException("BootstrapServers configuration is missing.");


        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<KafkaTopicsInitializer>>();
            return new KafkaTopicsInitializer(bootstrapServers, logger);
        });

        services.AddHostedService<BookingProcessRequestTopicInitializingService>();

        services.AddSingleton(sp =>
        {
            return new BookingProcessResponseProducer(bootstrapServers);
        });

        services.AddSingleton<IHostedService>(sp =>
        {
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var producer = sp.GetRequiredService<BookingProcessResponseProducer>();
            var logger = sp.GetRequiredService<ILogger<BookingProcessRequestBackgroundService>>();

            var groupId = config.GetRequiredSection("BookingProcessRequestGroupId").Value ?? 
                throw new InvalidOperationException("GroupId configuration is missing.");

            return new BookingProcessRequestBackgroundService(
                scopeFactory,
                producer,
                logger,
                bootstrapServers,
                groupId
            );
        });


        return services;
    }
}
