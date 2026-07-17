using EventManager.Infrastructure.DataAccess;
using EventManager.Application.Services;
using EventManager.Domain.Models;
using Moq;
using Microsoft.Extensions.Logging;
using EventManager.Domain.Exceptions;
using Microsoft.Extensions.Configuration;

namespace EventManager.Tests.Integration;

[Collection("Postgres collection")]
public class BookingServiceTests(PostgresFixture postgresFixture) : PostgresTest(postgresFixture)
{
    [Fact]
    [Trait("Category", "BookingService")]
    public async Task BookingPastEvent_ThrowsPastEventBookingException()
    {
        // Arrange
        await ResetDatabaseAsync();

        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var bookingRepository = new BookingRepository(actContext);
        var eventRepository = new EventRepository(actContext);
        var userRepository = new UserRepository(actContext);
        var mockLogger = new Mock<ILogger<BookingService>>();
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["User:MaxActiveBookings"]).Returns("10");
        var bookingService = new BookingService(bookingRepository, eventRepository, userRepository, mockLogger.Object, mockConfiguration.Object);

        var pastEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Past Event",
            Description = "This event is in the past.",
            StartAt = DateTime.UtcNow.AddDays(-1),
            EndAt = DateTime.UtcNow.AddDays(-1).AddHours(2),
            TotalSeats = 10
        };
        await eventRepository.CreateAsync(pastEvent);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "testuser",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };

        await userRepository.CreateAsync(user);

        // Act & Assert
        Assert.Throws<PastEventBookingException>(() => bookingService.CreateBookingAsync(pastEvent.Id, user.Id).GetAwaiter().GetResult());
    }

    [Fact]
    public async Task ExeedingActiveBookingLimit_ThrowsExceedingActiveBookingLimitException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var bookingRepository = new BookingRepository(actContext);
        var eventRepository = new EventRepository(actContext);
        var userRepository = new UserRepository(actContext);
        var mockLogger = new Mock<ILogger<BookingService>>();
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["User:MaxActiveBookings"]).Returns("10");
        var bookingService = new BookingService(bookingRepository, eventRepository, userRepository, mockLogger.Object, mockConfiguration.Object);
        var futureEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Future Event",
            Description = "This event is in the future.",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            TotalSeats = 10
        };
        await eventRepository.CreateAsync(futureEvent);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "testuser",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };
        await userRepository.CreateAsync(user);
        // Create 10 bookings for the user
        for (int i = 0; i < 10; i++)
        {
            await bookingService.CreateBookingAsync(futureEvent.Id, user.Id);
        }
        // Act & Assert
        Assert.Throws<ExceedingActiveBookingLimitException>(() => bookingService.CreateBookingAsync(futureEvent.Id, user.Id).GetAwaiter().GetResult());
        Assert.Equal(10, (await bookingService.GetBookingsByEventIdAsync(futureEvent.Id)).Count);
        Assert.Equal(0, (await eventRepository.GetByIdAsync(futureEvent.Id)).AvailableSeats);
    }

    [Fact]
    public async Task UserLimitsDoNotAffectEachOther()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var bookingRepository = new BookingRepository(actContext);
        var eventRepository = new EventRepository(actContext);
        var userRepository = new UserRepository(actContext);
        var mockLogger = new Mock<ILogger<BookingService>>();
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["User:MaxActiveBookings"]).Returns("10");
        var bookingService = new BookingService(bookingRepository, eventRepository, userRepository, mockLogger.Object, mockConfiguration.Object);
        var futureEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Future Event",
            Description = "This event is in the future.",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            TotalSeats = 20
        };
        await eventRepository.CreateAsync(futureEvent);
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Login = "testuser1",
            PasswordHash = "hashedpassword1",
            Role = UserRole.User
        };
        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Login = "testuser2",
            PasswordHash = "hashedpassword2",
            Role = UserRole.User
        };
        await userRepository.CreateAsync(user1);
        await userRepository.CreateAsync(user2);

        // Act
        // Create 10 bookings for user1
        for (int i = 0; i < 10; i++)
        {
            await bookingService.CreateBookingAsync(futureEvent.Id, user1.Id);
        }
        // Create 5 bookings for user2
        for (int i = 0; i < 5; i++)
        {
            await bookingService.CreateBookingAsync(futureEvent.Id, user2.Id);
        }

        // Assert
        Assert.Throws<ExceedingActiveBookingLimitException>(() => bookingService.CreateBookingAsync(futureEvent.Id, user1.Id).GetAwaiter().GetResult());
        Assert.Equal(10, (await bookingService.GetBookingsByEventIdAsync(futureEvent.Id)).Count(b => b.UserId == user1.Id));
        Assert.Equal(5, (await bookingService.GetBookingsByEventIdAsync(futureEvent.Id)).Count(b => b.UserId == user2.Id));
        await bookingService.CreateBookingAsync(futureEvent.Id, user2.Id); // This should succeed
        Assert.Equal(6, (await bookingService.GetBookingsByEventIdAsync(futureEvent.Id)).Count(b => b.UserId == user2.Id));
    }
}
