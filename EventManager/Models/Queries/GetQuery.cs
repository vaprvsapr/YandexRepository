namespace EventManager.Models.Queries;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Модель данных для параметров запроса при получении событий.
/// </summary>
public class GetQuery
{
    /// <summary>
    /// Подстрока для фильтрации по названию.
    /// </summary>
    public string? Title { get; init; } = null;

    private DateTime? _from = null;
    /// <summary>
    /// Время начала фильтрации.
    /// </summary>
    public DateTime? From 
    { 
        get { return _from; }
        init
        {
            if (_to == null || value < _to)
                _from = value;
            else
                throw new ValidationException("Время начала фильтрации должно быть меньше времени окончания фильтрации.");
        }
    }

    private DateTime? _to = null;
    /// <summary>
    /// Время окончания фильтрации.
    /// </summary>
    public DateTime? To 
    { 
        get { return _to; }
        init
        {
            if (_from == null || _from < value)
                _to = value;
            else
                throw new ValidationException("Время окончания фильтрации должно быть больше времени начала фильтрации.");
        }
    }

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
}
