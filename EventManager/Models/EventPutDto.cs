using System.ComponentModel.DataAnnotations;

namespace EventManager.Models;

/// <summary>
/// DTO модель данных события
/// </summary>
public class EventPutDto
{
    /// <summary>
    /// Название события, обязательное для заполнения, не должно быть пустой строкой
    /// </summary>
    public string? Title { get; set; } = null;

    /// <summary>
    /// Описание события, необязательное поле, может быть пустой строкой
    /// </summary>
    public string? Description { get; set; } = null;

    private DateTime? _startAt = null;
    /// <summary>
    /// Время начала события, обязательное для заполнения, должно быть меньше времени окончания события
    /// </summary>

    public DateTime? StartAt
    {
        get { return _startAt; }
        set
        {
            if (_endAt == null || value < _endAt)
                _startAt = value;
            else
                throw new ValidationException("Время начала события должно быть меньше времени окончания события.");
        }
    }

    private DateTime? _endAt = null;
    /// <summary>
    /// Время окончания события, обязательное для заполнения, должно быть больше времени начала события
    /// </summary>
    public DateTime? EndAt
    {
        get { return _endAt; }
        set
        {
            if (_startAt == null || _startAt < value)
                _endAt = value;
            else
                throw new ValidationException("Время окончания события должно быть больше времени начала события.");
        }
    }
}
