namespace EventManager.Models.Bookings;

public class Booking
{
    public required int Id { get; init; }
    public required int EventId { get; init; }
    public required BookingStatus Status { get; set; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; set; }
}
