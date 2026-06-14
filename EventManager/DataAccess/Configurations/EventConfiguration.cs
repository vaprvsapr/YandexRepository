using EventManager.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.DataAccess.Configurations;

/// <summary>
/// Класс конфигурации для сущности Event, определяющий структуру таблицы "events" и ее свойства,
/// </summary>
public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    /// <summary>
    /// Метод конфигурации, который задает правила для отображения сущности Event в базе данных, 
    /// включая имена столбцов, типы данных, ограничения и связи с другими сущностями.
    /// </summary>
    /// <param name="builder"></param>
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(b => b.StartAt)
            .HasColumnName("start_at")
            .IsRequired();

        builder.Property(b => b.EndAt)
            .HasColumnName("end_at")
            .IsRequired();

        builder.Property(b => b.Description)
            .HasColumnName("description")
            .HasMaxLength(50);

        builder.Property(b => b.AvailableSeats)
            .HasColumnName("abailable_seats")
            .HasMaxLength(10000)
            .IsRequired();

        builder.Property(b => b.TotalSeats)
            .HasColumnName("total_seats")
            .HasMaxLength(10000)
            .IsRequired();

        builder.Property(b => b.Title)
            .HasColumnName("title")
            .HasMaxLength(50)
            .IsRequired();

        builder
            .HasMany(e => e.Bookings)
            .WithOne(b => b.Event)
            .HasForeignKey(b => b.EventId)
            .IsRequired();
    }
}
