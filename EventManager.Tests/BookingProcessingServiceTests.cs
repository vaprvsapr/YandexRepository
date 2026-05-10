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
            NumberOfSeats = 2
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
        await service.ProcessBooking(bookings[0], 10, TestContext.Current.CancellationToken);
        await service.ProcessBooking(bookings[1], 10, TestContext.Current.CancellationToken);
        await service.ProcessBooking(bookings[2], 10, TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(2, bookings.Count(b => b.Status == BookingStatus.Confirmed));
        Assert.Equal(1, bookings.Count(b => b.Status == BookingStatus.Rejected));
    }
}
