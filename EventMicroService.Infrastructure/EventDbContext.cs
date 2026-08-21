using EventMicroService.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventMicroService.Infrastructure;

/// <summary>
/// Контекст базы данных.
/// </summary>
/// <remarks>
/// Конструктор для AppDbContext, принимающий параметры конфигурации базы данных и передающий их базовому классу DbContext.
/// </remarks>
/// <param name="options"></param>
public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{

    /// <summary>
    /// Коллекция событий, представляющая таблицу в базе данных для хранения информации о событиях.
    /// </summary>
    public DbSet<Event> Events => Set<Event>();

    public DbSet<EventBooking> EventBookings => Set<EventBooking>();

    /// <summary>
    /// Метод для настройки модели данных и определения конфигурации сущностей при создании модели базы данных. 
    /// Он применяет все конфигурации, определенные в сборке, содержащей AppDbContext,
    /// что позволяет централизованно управлять настройками модели данных.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
    }
}
