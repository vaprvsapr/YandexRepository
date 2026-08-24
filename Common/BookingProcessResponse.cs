namespace Common;

public class BookingProcessResponse
{
    public Guid BookingId { get; set; }
    public BookingProcessResult Result { get; set; }
}

public enum BookingProcessResult
{
    Confirmed,
    Cancelled,
    Error
}