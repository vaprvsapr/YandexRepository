using EventManager.DataAccess;
using EventManager.DataAccess.Repositories;
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
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        // Сервис событий и его репозиторий
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventService, EventService>();

        // Сервис бронирования и его репозиторий
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingService, BookingService>();

        // Фоновый сервис для обработки бронирований
        services.AddHostedService<BookingProcessingService>();

        return services;
    }
}
