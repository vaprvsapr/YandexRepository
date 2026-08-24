using Common.Contracts;

namespace BookingMicroService.Application;

/// <summary>
/// Интерфейс для публикации запросов на обработку бронирования в систему.
/// </summary>
public interface IBookingProcessRequestProducer : IDisposable
{
    /// <summary>
    /// Метод публикации запросов.
    /// </summary>
    /// <param name="bookingProcessRequest">Запрос на создание бронирования</param>
    /// <returns></returns>
    Task PublishAsync(BookingProcessRequest bookingProcessRequest);
}
