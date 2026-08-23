using Common;

namespace BookingMicroService.Application;

public interface IBookingCreatedProducer
{
    void Dispose();
    Task PublishAsync(BookingProcessRequest bookingCreated);
}
