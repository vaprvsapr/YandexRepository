using UserMicroService.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UserMicroService.Infrastructure;

/// <summary>
/// Методы расширения для регистрации зависимостей в контейнере внедрения зависимостей.
/// </summary>
public static partial class DependencyInjectionExtensions
{
    /// <summary>
    /// Добавляет инфраструктурные сервисы в коллекцию служб приложения.
    /// </summary>
    public static IServiceCollection AddUserInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("UserDbConnection"));
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenGeneratingService, TokenGeneratingService>();

        return services;
    }
}