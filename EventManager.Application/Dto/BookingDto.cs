using EventManager.Domain.Models;

namespace EventManager.Application.Dto;

/// <summary>
/// Модель данных для представления бронирования события.
/// </summary>
public class BookingDto
{
    /// <summary>
    /// Идентификатор бронирования.
    /// </summary>
    public Guid Id { get; init; } = Guid.Empty;

    /// <summary>
    /// Идентификатор события.
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// Идентификатор пользователя, который сделал бронирование.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Статус бронирования.
    /// </summary>
    public BookingStatus Status { get; init; } = BookingStatus.Pending;

    /// <summary>
    /// Дата и время создания бронирования.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время обработки бронирования.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
}
