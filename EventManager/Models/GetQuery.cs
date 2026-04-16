namespace EventManager.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Модель данных для параметров запроса при получении событий.
/// </summary>
public class GetQuery : IValidatableObject
{
    /// <summary>
    /// Подстрока для фильтрации по названию.
    /// </summary>
    public string? Title { get; init; } = null;

    /// <summary>
    /// Время начала фильтрации.
    /// </summary>
    public DateTime? From { get; init; } = null;

    /// <summary>
    /// Время окончания фильтрации.
    /// </summary>
    public DateTime? To { get; init; } = null;

    /// <summary>
    /// Номер страницы для отображения.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть больше 0.")]
    public int Page { get; init; } = 1;

    /// <summary>
    /// Колечество элементов на странице для отображения.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Количество элементов на странице должно быть между 1 и 100.")]
    public int PageSize { get; init; } = 10;

    IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
    {
        if (From != null && To != null && From >= To)
            yield return new ValidationResult(
                "Время окончания события должно быть позже времени начала.",
                [nameof(From)]
            );
    }
}
