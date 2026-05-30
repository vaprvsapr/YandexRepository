using EventManager.Models.Bookings;
using EventManager.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace EventManager.DataAccess;

/// <summary>
/// Контекст базы данных.
/// </summary>
internal sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
