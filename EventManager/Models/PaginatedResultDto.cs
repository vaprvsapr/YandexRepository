namespace EventManager.Models;
using EventManager.Models;

/// <summary>
/// Модель данных для представления результатов пагинации при получении списка событий.
/// </summary>
public class PaginatedResultDto
{
    /// <summary>
    /// События на странице.
    /// </summary>
    public IEnumerable<EventDto>? Events { get; init; }

    /// <summary>
    /// Общее число событий, удовлетворяющих условиям фильтрации.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Количество элементов страницы.
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Номер страницы.
    /// </summary>
    public int Page { get; init; } = 1;

}
