using System.ComponentModel.DataAnnotations;

namespace EventManager.Models.Events;

/// <summary>
/// Базовая DTO модель данных события.
/// </summary>
public abstract class EventBaseDto : IValidatableObject
{
    /// <summary>
    /// Название события, обязательное для заполнения, не должно быть пустой строкой
    /// </summary>
    [Required(ErrorMessage = "title обязателен для заполнения.")]
    [MinLength(1, ErrorMessage = "title не может быть пустой строкой.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание события, необязательное поле, может быть пустой строкой
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Время начала события, обязательное для заполнения, должно быть меньше времени окончания события
    /// </summary>
    [Required(ErrorMessage = "startAt обязателен для заполнения.")]
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// Время окончания события, обязательное для заполнения, должно быть больше времени начала события
    /// </summary>
    [Required(ErrorMessage = "endAt обязателен для заполнения.")]
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// Количество мест на событие, обязательное поле
    /// </summary>
    [Required(ErrorMessage = "TotalSeats обязательно для заполнения.")]
    [Range(1, 10000, ErrorMessage = "Количество мест должно быть больше нуля.")]
    public int TotalSeats { get; set; }

    /// <summary>
    /// Выполняет проверку объекта на соответствие бизнес-правилам и возвращает результаты проверки
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
