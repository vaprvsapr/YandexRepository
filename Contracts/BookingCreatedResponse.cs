namespace Contracts;

public class BookingCreatedResponse
{
    public Guid BookingId { get; set; }
    public bool Confirmed { get; set; }
}
