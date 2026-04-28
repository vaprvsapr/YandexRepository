namespace EventManager.Models.Events;

/// <summary>
/// Модель данных события.
/// </summary>
public class Event
{
    /// <summary>
    /// Идентификатор события.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Название события.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Описание события.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Время начала события.
    /// </summary>
    public required DateTime? StartAt { get; set; }

    /// <summary>
    /// Время окончания события.
    /// </summary>
    public required DateTime? EndAt { get; set; }
}
