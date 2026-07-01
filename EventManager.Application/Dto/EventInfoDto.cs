using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.Dto;

/// <summary>
/// DTO модель данных события
/// </summary>
public class EventInfoDto : EventBaseDto
{
    /// <summary>
    /// ID события, обязательное для заполнения при обновлении и удалении   
    /// </summary>
    [Required(ErrorMessage = "id обязателен для заполнения.")]
    public Guid Id { get; set; }

    /// <summary>
    /// Количество свободных мест на событие.
    /// </summary>
    [Range(0, 10000, ErrorMessage = "Количество свободных мест не может быть отрицательным.")]
    public int AvailableSeats { get; set; }
}