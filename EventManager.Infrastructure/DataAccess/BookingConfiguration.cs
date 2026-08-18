using EventManager.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Infrastructure.DataAccess;

/// <summary>
/// Класс конфигурации для сущности Booking.
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    /// <summary>
    /// Метод конфигурации сущности Booking, определяющий структуру таблицы "bookings" и ее свойства, 
    /// а также устанавливающий связи с сущностью Event.
    /// </summary>
    /// <param name="builder"></param>
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(b => b.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(b => b.EventId)
            .HasColumnName("event_id");

        builder.Property(b => b.UserId)
            .HasColumnName("user_id");

        builder.HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
