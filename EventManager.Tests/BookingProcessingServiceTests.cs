using Moq;
using EventManager.Interfaces;
using EventManager.Models.Bookings;
using EventManager.Models.Events;
using EventManager.Services;
using Microsoft.Extensions.Logging;

namespace EventManager.Tests;

public class BookingProcessingServiceTests
{
    [Fact]
    [Trait("Category", "BookingProcessingService")]
    public async Task ExecuteAsync_ShouldProcessPendingBooking()
    {
        // Arrange
        var bookingRepositoryMock = new Mock<IRepository<Booking>>();
        var eventRepositoryMock = new Mock<IRepository<Event>>();
        var loggerMock = new Mock<ILogger<BookingProcessingService>>();

        var eventId = Guid.NewGuid();
        Event eventWithSeats = new()
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 2
        };

        List<Booking> bookings =
        [
            new()
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Confirmed,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now
            }
        ];

        bookingRepositoryMock.Setup(repo => repo.GetAll()).Returns(bookings);
        eventRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).Returns(eventWithSeats);

        var service = new BookingProcessingService(bookingRepositoryMock.Object, eventRepositoryMock.Object, loggerMock.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        // Act
        var tasks = bookings.Select(b => service.ProcessBookingAsync(b, 10, TestContext.Current.CancellationToken));  
        await Task.WhenAll(tasks);
        // Assert
        Assert.Equal(3, bookings.Count(b => b.Status == BookingStatus.Confirmed));
    }
}
