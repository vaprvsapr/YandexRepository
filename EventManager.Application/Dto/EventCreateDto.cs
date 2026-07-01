using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.Dto;

/// <summary>
/// DTO модель данных события
/// </summary>
public class EventCreateDto : EventBaseDto
{
    /// <summary>
    /// ID события, обязательное для заполнения при обновлении и удалении   
    /// </summary>
    [Required(ErrorMessage = "id обязателен для заполнения.")]
    public Guid Id { get; set; }
}