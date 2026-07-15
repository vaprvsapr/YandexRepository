namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение, которое выбрасывается, когда пользователь пытается создать бронирование, превышающее лимит активных бронирований.
/// </summary>
/// <param name="message"></param>
internal class ExceedingActiveBookingLimitException(string message) : Exception(message)
{
}
