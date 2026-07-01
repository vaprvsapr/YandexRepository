using System.Reflection;

namespace EventManager.Presentation.DI;

/// <summary>
/// Класс расширений для внедрения зависимостей, связанных с визуализацией и документацией API, в коллекцию служб приложения.
/// </summary>
public static partial class DependencyInjectionExtensions
{
    /// <summary>
    /// Добавляет сервисы для визуализации и документации API в коллекцию служб приложения.
    /// </summary>
    public static IServiceCollection AddVisualization(this IServiceCollection services)
    {
        // Регистрация Swagger для документации API
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // Путь к XML-файлу с документацией
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });
        return services;
    }
}
