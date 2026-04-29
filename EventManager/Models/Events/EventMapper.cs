namespace EventManager.Models.Events;


/// <summary>
/// Маппер для преобразования между DTO моделью данных события и моделью данных события.
/// </summary>
public class EventMapper
{
    /// <summary>
    /// Метод преобразования DTO модели данных события в модель данных события, используемую в бизнес-логике приложения
    /// </summary>
    /// <param name="eventDto"></param>
    /// <returns></returns>
    public static Event ToEvent(EventDto eventDto)
    {
        return new Event
        {
            Id = eventDto.Id, // problem with ??
            Title = eventDto.Title,
            Description = eventDto.Description,
            StartAt = eventDto.StartAt,
            EndAt = eventDto.EndAt
        };
    }

    /// <summary>
    /// Метод преобразования модели данных события, используемой в бизнес-логике приложения, в DTO модель данных события, используемую для передачи данных между слоями приложения и клиентом
    /// </summary>
    /// <param name="eventModel"></param>
    /// <returns></returns>
    public static EventDto ToEventDto(Event eventModel)
    {
        return new EventDto
        {
            Id = eventModel.Id,
            Title = eventModel.Title,
            Description = eventModel.Description,
            StartAt = eventModel.StartAt,
            EndAt = eventModel.EndAt
        };
    }
}
