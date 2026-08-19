namespace Contracts;

public class BookingCreated
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
}
