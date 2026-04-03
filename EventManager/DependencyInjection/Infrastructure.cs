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

        // Конфигурация обработки ошибок валидации модели
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    var apiResult = new EventManager.Models.ApiResult
                    {
                        Success = false,
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = string.Join("; ", errors),
                        DateTime = DateTime.Now
                    };

                    return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(apiResult);
                };
            });

        return services;
    }
}
