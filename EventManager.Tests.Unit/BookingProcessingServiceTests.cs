using EventManager.Domain.Models;
using EventManager.Application.Repositories;
using EventManager.Infrastructure.Services;
using EventManager.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventManager.Tests.Unit;

public class BookingProcessingServiceTests
{
    [Fact]
    [Trait("Category", "BookingProcessingService")]
    public async Task ExecuteAsync_ShouldProcessPendingBooking()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(dbOptions);

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

        context.Events.Add(eventWithSeats);
        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync(CancellationToken.None);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IBookingRepository))).Returns(new BookingRepository(context));
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventRepository))).Returns(new EventRepository(context));
    

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var loggerMock = new Mock<ILogger<BookingProcessingService>>();

        var service = new BookingProcessingService(scopeFactoryMock.Object, loggerMock.Object);

        // Act
        var tasks = bookings.Select(b => service.ProcessBookingAsync(b.Id, CancellationToken.None));
        await Task.WhenAll(tasks);

        // Assert
        var updatedBookings = await context.Bookings.ToListAsync(CancellationToken.None);
        Assert.Equal(3, updatedBookings.Count(b => b.Status == BookingStatus.Confirmed));
    }
}
