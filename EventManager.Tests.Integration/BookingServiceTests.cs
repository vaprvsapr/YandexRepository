using EventManager.Infrastructure.DataAccess;
using EventManager.Application.Services;
using EventManager.Domain.Models;
using Moq;
using Microsoft.Extensions.Logging;
using EventManager.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using EventManager.Application.Dto;
using Org.BouncyCastle.Bcpg;

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
        Assert.Throws<PastEventBookingException>(() => bookingService.CreateAsync(pastEvent.Id, user.Id).GetAwaiter().GetResult());
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
            await bookingService.CreateAsync(futureEvent.Id, user.Id);
        }
        // Act & Assert
        Assert.Throws<ExceedingActiveBookingLimitException>(() => 
        bookingService.CreateAsync(futureEvent.Id, user.Id).GetAwaiter().GetResult());
        Assert.Equal(0, (await eventRepository.GetByIdAsync(futureEvent.Id))?.AvailableSeats);
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
            await bookingService.CreateAsync(futureEvent.Id, user1.Id);
        }
        // Create 5 bookings for user2
        for (int i = 0; i < 5; i++)
        {
            await bookingService.CreateAsync(futureEvent.Id, user2.Id);
        }

        // Assert
        await Assert.ThrowsAsync<ExceedingActiveBookingLimitException>(async () => await bookingService.CreateAsync(futureEvent.Id, user1.Id));
        Assert.Equal(10, (await bookingService.GetAllBookingsAsync()).Count(b => b.UserId == user1.Id));
        Assert.Equal(5, (await bookingService.GetAllBookingsAsync()).Count(b => b.UserId == user2.Id));
        await bookingService.CreateAsync(futureEvent.Id, user2.Id); // This should succeed
        Assert.Equal(6, (await bookingService.GetAllBookingsAsync()).Count(b => b.UserId == user2.Id));
    }

    [Fact]
    public async Task CancelByIdAsync_CancellingValidBooking_CancellsBooking()
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

        var eventId = Guid.NewGuid();
        var @event = new Event
        {
            Id = eventId,
            Title = "Future Event",
            Description = "This event is in the future.",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            TotalSeats = 20
        };
        await eventRepository.CreateAsync(@event);

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Login = "login",
            Role = UserRole.User,
            PasswordHash = "hash"
        };
        var userInfoDto = new UserInfoDto
        {
            Id = userId,
            Login = user.Login,
            Role = user.Role.ToString()
        };
        await userRepository.CreateAsync(user);

        // Act, Assert
        var bookingDto = await bookingService.CreateAsync(eventId, userId);

        await bookingService.CancelByIdAsync(bookingDto.Id, userInfoDto);
        Assert.Equal(BookingStatus.Cancelled, (await bookingService.GetByIdAsync(bookingDto.Id))?.Status);
    }

    [Fact]
    public async Task CancelByIdAsync_CancellingOtherUserBooking_ThrowsException()
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

        var eventId = Guid.NewGuid();
        var @event = new Event
        {
            Id = eventId,
            Title = "Future Event",
            Description = "This event is in the future.",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            TotalSeats = 20
        };
        await eventRepository.CreateAsync(@event);

        var firstUserId = Guid.NewGuid();
        var firstUser = new User
        {
            Id = firstUserId,
            Login = "firstUserLogin",
            Role = UserRole.User,
            PasswordHash = "hash"
        };
        var secondUserId = Guid.NewGuid();
        var secondUser = new User
        {
            Id = secondUserId,
            Login = "secondUserLogin",
            Role = UserRole.User,
            PasswordHash = "hash"
        };
        var secondUserInfoDto = new UserInfoDto
        {
            Id = secondUserId,
            Login = secondUser.Login,
            Role = secondUser.Role.ToString()
        };

        await userRepository.CreateAsync(firstUser);
        await userRepository.CreateAsync(secondUser);

        // Act, Assert
        var bookingDto = await bookingService.CreateAsync(eventId, firstUserId);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => { await bookingService.CancelByIdAsync(bookingDto.Id, secondUserInfoDto); });
    }

    [Fact]
    public async Task CancelByIdAsync_AdminCancellingOtherUserBooking_CancellsBooking()
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

        var eventId = Guid.NewGuid();
        var @event = new Event
        {
            Id = eventId,
            Title = "Future Event",
            Description = "This event is in the future.",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            TotalSeats = 20
        };
        await eventRepository.CreateAsync(@event);

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Login = "firstUserLogin",
            Role = UserRole.User,
            PasswordHash = "hash"
        };
        var adminId = Guid.NewGuid();
        var admin = new User
        {
            Id = adminId,
            Login = "adminLogin",
            Role = UserRole.Admin,
            PasswordHash = "hash"
        };
        var adminUserInfoDto = new UserInfoDto
        {
            Id = adminId,
            Login = admin.Login,
            Role = admin.Role.ToString()
        };

        await userRepository.CreateAsync(user);
        await userRepository.CreateAsync(admin);

        // Act, Assert
        var bookingDto = await bookingService.CreateAsync(eventId, userId);

        await bookingService.CancelByIdAsync(bookingDto.Id, adminUserInfoDto);
        Assert.Equal(BookingStatus.Cancelled, (await bookingService.GetByIdAsync(bookingDto.Id))?.Status);
    }
}
