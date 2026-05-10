namespace EventManager.Models.Bookings;

/// <summary>
/// Модель данных бронирования события.
/// </summary>
public class Booking
{
    /// <summary>
    /// Идентификатор бронирования.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Идентификатор события, к которому относится бронирование.
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// Статус бронирования.
    /// </summary>
    public required BookingStatus Status { get; set; }

    /// <summary>
    /// Дата и время создания бронирования.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время обработки бронирования.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
}
