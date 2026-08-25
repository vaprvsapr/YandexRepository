namespace EventMicroService.Domain;

public class EventBooking
{
    public required Guid EventId { get; set; }
    public required Guid BookingId { get; set; }
}
