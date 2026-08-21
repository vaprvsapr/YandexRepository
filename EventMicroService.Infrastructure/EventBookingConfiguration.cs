using EventMicroService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventMicroService.Infrastructure;

/// <summary>
/// Класс конфигурации для сущности Event, определяющий структуру таблицы "events" и ее свойства,
/// </summary>
public class EventBookingConfiguration : IEntityTypeConfiguration<EventBooking>
{
    /// <summary>
    /// Метод конфигурации, который задает правила для отображения сущности Event в базе данных, 
    /// включая имена столбцов, типы данных, ограничения и связи с другими сущностями.
    /// </summary>
    /// <param name="builder"></param>
    public void Configure(EntityTypeBuilder<EventBooking> builder)
    {
        builder.ToTable("events_bookings");

        builder.HasKey(b => b.BookingId);

        builder.Property(b => b.BookingId)
            .HasColumnName("booking_id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(b => b.EventId)
            .HasColumnName("event_id")
            .IsRequired();
    }
}