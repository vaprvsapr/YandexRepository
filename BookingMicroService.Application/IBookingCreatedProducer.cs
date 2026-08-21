using Contracts;

namespace BookingMicroService.Application;

public interface IBookingCreatedProducer
{
    Task PublishAsync(BookingProcessRequest bookingCreated);
}
