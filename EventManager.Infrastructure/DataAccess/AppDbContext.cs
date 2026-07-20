using EventManager.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.DataAccess;

/// <summary>
/// Контекст базы данных.
/// </summary>
/// <remarks>
/// Конструктор для AppDbContext, принимающий параметры конфигурации базы данных и передающий их базовому классу DbContext.
/// </remarks>
/// <param name="options"></param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    /// <summary>
    /// Коллекция событий, представляющая таблицу в базе данных для хранения информации о событиях.
    /// </summary>
    public DbSet<Event> Events => Set<Event>();

    /// <summary>
    /// Коллекция бронирований, представляющая таблицу в базе данных для хранения информации о бронированиях событий.
    /// </summary>
    public DbSet<Booking> Bookings => Set<Booking>();

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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
