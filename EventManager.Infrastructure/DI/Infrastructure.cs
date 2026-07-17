using EventManager.Infrastructure.DataAccess;
using EventManager.Infrastructure.Services;
using EventManager.Application.Repositories;
using EventManager.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EventManager.Application.Services.Interfaces;

namespace EventManager.Infrastructure.DI;

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

        // Сервис пользователей и его репозиторий
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<ITokenGeneratingService, TokenGeneratingService>();

        // Фоновый сервис для обработки бронирований
        services.AddHostedService<BookingProcessingService>();

        

        return services;
    }
}
