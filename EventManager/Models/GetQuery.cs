namespace EventManager.Models;

using System.ComponentModel.DataAnnotations;

public class GetQuery : IValidatableObject
{
    public string? Title { get; init; } = null;

    public DateTime? From { get; init; } = null;

    public DateTime? To { get; init; } = null;

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0.")]
    public int Page { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
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
