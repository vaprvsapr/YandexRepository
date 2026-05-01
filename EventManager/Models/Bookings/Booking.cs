namespace EventManager.Models.Bookings;

public class Booking
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required BookingStatus Status { get; set; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime ProcessedAt { get; set; }
}
