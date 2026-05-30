using EventManager.Models.Bookings;

namespace EventManager.Models.Events;

/// <summary>
/// Модель данных события.
/// </summary>
public class Event
{
    /// <summary>
    /// Идентификатор события.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Название события.
    /// </summary>
    public required string Title { get; set; } = null!;

    /// <summary>
    /// Описание события.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Время начала события.
    /// </summary>
    public required DateTime? StartAt { get; set; }

    /// <summary>
    /// Время окончания события.
    /// </summary>
    public required DateTime? EndAt { get; set; }

    private readonly int _totalSeats;
    /// <summary>
    /// Количество мест на событие, обязательное поле.
    /// </summary>
    public required int TotalSeats 
    {
        get => _totalSeats;
        init
        {
            _totalSeats = value;
            AvailableSeats = value;
        }
    }

    /// <summary>
    /// Количество свободных мест на событие.
    /// </summary>
    public int AvailableSeats { get; set; }

    /// <summary>
    /// Метод для попытки зарезервировать указанное количество мест на событие. 
    /// Если доступно достаточное количество мест, то они резервируются (уменьшается количество свободных мест)
    /// и возвращается true. Если свободных мест недостаточно, метод возвращает false и не изменяет количество свободных мест.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats >= count)
        {
            AvailableSeats -= count;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Метод для освобождения указанного количества мест на событие. 
    /// Увеличивает количество свободных мест на указанное значение,
    /// </summary>
    /// <param name="count"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void ReleaseSeats(int count = 1)
    {
        if (AvailableSeats + count <= TotalSeats)
            AvailableSeats += count;
        else throw new InvalidOperationException("Нельзя освободить больше мест, чем было изначально.");
    }

    /// <summary>
    /// Список бронирований этого события.
    /// </summary>
    public List<Booking> Bookings { get; set; } = null!;
}
