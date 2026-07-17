namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение, которое выбрасывается, когда пользователь пытается войти в систему с неправильным паролем.
/// </summary>
/// <param name="message"></param>
public class WrongPasswordException(string message) : Exception(message)
{
}
