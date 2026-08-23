namespace Common;

public class BookingProcessRequest
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public BookingProcessCommand Command { get; set; }
}

public enum BookingProcessCommand
{
    Confirm,
    Cancel
}