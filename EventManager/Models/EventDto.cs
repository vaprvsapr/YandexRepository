using System.ComponentModel.DataAnnotations;

namespace EventManager.Models;

/// <summary>
/// DTO модель данных события
/// </summary>
public class EventDto
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

    private DateTime? _startAt;

    /// <summary>
    /// Время начала события, обязательное для заполнения, должно быть меньше времени окончания события
    /// </summary>
    [Required(ErrorMessage = "startAt обязателен для заполнения.")]
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

    private DateTime? _endAt;

    /// <summary>
    /// Время окончания события, обязательное для заполнения, должно быть больше времени начала события
    /// </summary>
    [Required(ErrorMessage = "endAt обязателен для заполнения.")]
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