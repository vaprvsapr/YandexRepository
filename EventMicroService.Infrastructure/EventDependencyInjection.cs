using EventMicroService.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

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
        services.AddScoped<IEventService, EventService>();

        return services;
    }
}
