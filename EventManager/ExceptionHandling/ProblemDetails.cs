namespace EventManager.ExceptionHandling;

/// <summary>
/// Стандартная модель данных для передачи информации об ошибке 
/// в формате JSON в ответ на HTTP запросы, которые вызвали исключение.
/// </summary>
public class ProblemDetails
{
    /// <summary>
    /// Тип ошибки.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Заголовок ошибки.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// HTTP статус ошибки.
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Сообщение исключения.
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// Экземпляр ошибки.
    /// </summary>
    public string? Instance { get; set; }
}
