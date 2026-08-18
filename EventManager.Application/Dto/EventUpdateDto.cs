using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.Dto;

/// <summary>
/// DTO модель данных события для обновления существующего события.
/// </summary>
public class EventUpdateDto
{
    /// <summary>
    /// Название события, обязательное для заполнения, не должно быть пустой строкой
    /// </summary>
    [MinLength(1, ErrorMessage = "title не может быть пустой строкой.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание события, необязательное поле, может быть пустой строкой
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Время начала события, обязательное для заполнения, должно быть меньше времени окончания события
    /// </summary>
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// Время окончания события, обязательное для заполнения, должно быть больше времени начала события
    /// </summary>
    public DateTime? EndAt { get; set; }
}