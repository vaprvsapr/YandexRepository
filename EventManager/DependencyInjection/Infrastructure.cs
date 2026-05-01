using EventManager.Data;
using EventManager.Interfaces;
using EventManager.Services;

namespace EventManager.DependencyInjection;

/// <summary>
/// Методы расширения для регистрации зависимостей в контейнере внедрения зависимостей.
/// </summary>
public static partial class DependencyInjectionExtensions
{
    /// <summary>
    /// Добавляет инфраструктурные сервисы в коллекцию служб приложения.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRepository<Models.Events.Event>, EventRepository>();
        services.AddScoped<IEventService, EventService>();

        services.AddSingleton<IRepository<Models.Bookings.Booking>, BookingRepository>();
        services.AddScoped<IBookingService, BookingService>();

        services.AddHostedService<BookingProcessingService>();

        return services;
    }
}
