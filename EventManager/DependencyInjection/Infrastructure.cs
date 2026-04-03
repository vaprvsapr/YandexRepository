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
        services.AddSingleton<IEventRepository, EventRepository>();
        services.AddScoped<IEventService, EventService>();

        return services;
    }
}
