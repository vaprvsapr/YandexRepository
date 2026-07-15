using EventManager.Domain.Models;

namespace EventManager.Domain.Models;

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

    /// <summary>
    /// Метод позволяет подтвердить бронирование, устанавливая статус в "Подтверждено" и фиксируя время обработки.
    /// </summary>
    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.Now.ToUniversalTime();
    }

    /// <summary>
    /// Метод позволяет отклонить бронирование, устанавливая статус в "Отклонено" и фиксируя время обработки.
    /// </summary>
    public void Reject()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.Now.ToUniversalTime();
    }

    /// <summary>
    /// Объект события.
    /// </summary>
    public Event Event { get; set; } = null!;

}