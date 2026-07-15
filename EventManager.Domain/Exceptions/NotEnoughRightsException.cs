namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение, которое выбрасывается, когда пользователь пытается выполнить действие, на которое у него недостаточно прав.
/// </summary>
/// <param name="message"></param>
public class NotEnoughRightsException(string message) : Exception(message)
{
}
