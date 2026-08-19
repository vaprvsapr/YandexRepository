using BookingMicroService.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookingMicroService.Infrastructure;

/// <summary>
/// Контекст базы данных.
/// </summary>
/// <remarks>
/// Конструктор для AppDbContext, принимающий параметры конфигурации базы данных и передающий их базовому классу DbContext.
/// </remarks>
/// <param name="options"></param>
public class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{

    /// <summary>
    /// Коллекция бронирований, представляющая таблицу в базе данных для хранения информации о бронированиях событий.
    /// </summary>
    public DbSet<Booking> Bookings => Set<Booking>();

    /// <summary>
    /// Метод для настройки модели данных и определения конфигурации сущностей при создании модели базы данных. 
    /// Он применяет все конфигурации, определенные в сборке, содержащей AppDbContext,
    /// что позволяет централизованно управлять настройками модели данных.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
    }
}
