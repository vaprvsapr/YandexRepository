using EventManager.Data;
using EventManager.DataAccess;
using EventManager.Interfaces;
using EventManager.Services;
using Microsoft.EntityFrameworkCore;

namespace EventManager.DependencyInjection;

/// <summary>
/// Методы расширения для регистрации зависимостей в контейнере внедрения зависимостей.
/// </summary>
public static partial class DependencyInjectionExtensions
{
    /// <summary>
    /// Добавляет инфраструктурные сервисы в коллекцию служб приложения.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IRepository<Models.Events.Event>, EventInMemoryRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddSingleton<IRepository<Models.Bookings.Booking>, BookingInMemoryRepository>();
        services.AddScoped<IBookingService, BookingService>();

        services.AddHostedService<BookingProcessingService>();

        return services;
    }
}
