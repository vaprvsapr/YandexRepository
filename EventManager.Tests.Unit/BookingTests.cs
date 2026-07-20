using EventManager.Domain.Models;

namespace EventManager.Tests.Unit;

public class BookingTests
{
    [Fact]
    [Trait("Category", "Booking")]
    public void Confirm_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now
        };
        // Act
        booking.Confirm();
        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }


    [Fact]
    [Trait("Category", "Booking")]
    public void Reject_ShouldChangeStatusToReject()
    {
        // Arrange
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now
        };
        // Act
        booking.Reject();
        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }
}
