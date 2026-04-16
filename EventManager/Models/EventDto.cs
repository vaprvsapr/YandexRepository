using System.ComponentModel.DataAnnotations;

namespace EventManager.Models;

/// <summary>
/// DTO модель данных события
/// </summary>
public class EventDto : IValidatableObject
{
    /// <summary>
    /// ID события, обязательное для заполнения при обновлении и удалении, не должно быть изменяемым при создании
    /// </summary>
    [Required(ErrorMessage = "id обязателен для заполнения.")]
    public required int Id { get; init; }

    /// <summary>
    /// Название события, обязательное для заполнения, не должно быть пустой строкой
    /// </summary>
    [Required(ErrorMessage = "title обязателен для заполнения.")]
    public required string Title { get; set; }

    /// <summary>
    /// Описание события, необязательное поле, может быть пустой строкой
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Время начала события, обязательное для заполнения, должно быть меньше времени окончания события
    /// </summary>
    [Required(ErrorMessage = "startAt обязателен для заполнения.")]
    public required DateTime? StartAt { get; set; }

    /// <summary>
    /// Время окончания события, обязательное для заполнения, должно быть больше времени начала события
    /// </summary>
    [Required(ErrorMessage = "endAt обязателен для заполнения.")]
    public required DateTime? EndAt { get; set; }

    /// <summary>
    /// Выполняет проверку объекта на соответствие бизнес-правилам и возвращает результаты проверки.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndAt <= StartAt)
            yield return new ValidationResult(
                "Время окончания события должно быть позже времени начала.",
                [nameof(EndAt)]
            );
    }
}
