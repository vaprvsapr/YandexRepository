namespace EventManager.Domain.Models;

/// <summary>
/// Статус бронирования.
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// Обрабатывается.
    /// </summary>
    Pending,
    /// <summary>
    /// Подтверждено.
    /// </summary>
    Confirmed,
    /// <summary>
    /// Отклонено.
    /// </summary>
    Rejected,
    /// <summary>
    /// Отменено.
    /// </summary>
    Canceled
}

