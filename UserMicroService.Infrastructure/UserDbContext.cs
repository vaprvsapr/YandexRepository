using UserMicroService.Domain;
using Microsoft.EntityFrameworkCore;

namespace UserMicroService.Infrastructure;

/// <summary>
/// Контекст базы данных пользователей.
/// </summary>
/// <remarks>
/// Конструктор для AppDbContext, принимающий параметры конфигурации базы данных и передающий их базовому классу DbContext.
/// </remarks>
/// <param name="options"></param>
public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Коллекция пользователей, представляющая таблицу в базе данных для хранения информации о пользователях.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Метод для настройки модели данных и определения конфигурации сущностей при создании модели базы данных. 
    /// Он применяет все конфигурации, определенные в сборке, содержащей AppDbContext,
    /// что позволяет централизованно управлять настройками модели данных.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
    }
}
