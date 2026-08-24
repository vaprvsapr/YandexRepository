using Common;

namespace BookingMicroService.Application;

public interface IBookingProcessRequestProducer
{
    void Dispose();
    Task PublishAsync(BookingProcessRequest bookingCreated);
}
