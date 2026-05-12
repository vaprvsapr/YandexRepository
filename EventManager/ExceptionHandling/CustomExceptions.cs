namespace EventManager.ExceptionHandling;

/// <summary>
/// Исключение, которое выбрасывается, когда при попытке создать бронирование для события не осталось доступных мест.
/// </summary>
public class NoAvailableSeatsException(string message) : Exception(message) 
{
}
