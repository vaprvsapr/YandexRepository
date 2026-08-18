namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение, которое выбрасывается, когда пользователь пытается забронировать событие, которое уже прошло.
/// </summary>
/// <param name="message"></param>
public class PastEventBookingException(string message) : Exception(message)
{
}
