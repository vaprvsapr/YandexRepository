namespace EventManager.Models.Bookings;

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
    Rejected
}

