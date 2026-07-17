using EventManager.Domain.Models;
using EventManager.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Tests.Integration;

[Collection("Postgres collection")]
public class BookingRepositoryTests(PostgresFixture postgresFixture) : PostgresTest(postgresFixture)
{
    [Fact]
    [Trait("Category", "BookingRepository")]
    public async Task CreateAsync_ShouldCreateBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new BookingRepository(actContext);
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        arrangeContext.Events.Add(new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 10
        });
        arrangeContext.Users.Add(new User
        {
            Id = userId,
            Login = "testuser",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        });
        await arrangeContext.SaveChangesAsync();
        // Act
        var booking = await repository.CreateAsync(eventId, userId);
        // Assert
        Assert.NotNull(booking);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    [Trait("Category", "BookingRepository")]
    public async Task CreateAsync_WithInvalidEventId_ThrowsKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new BookingRepository(actContext);
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await repository.CreateAsync(eventId, userId));
    }

    [Fact]
    [Trait("Category", "BookingRepository")]
    public async Task GetByIdAsync_WithValidBookingId_ReturnBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new BookingRepository(actContext);
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        arrangeContext.Events.Add(new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 10
        });
        arrangeContext.Users.Add(new User
        {
            Id = userId,
            Login = "testuser",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        });

        await arrangeContext.SaveChangesAsync();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now.ToUniversalTime()
        };
        arrangeContext.Bookings.Add(booking);
        await arrangeContext.SaveChangesAsync();
        // Act
        var result = await repository.GetByIdAsync(booking.Id);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
    }

    [Fact]
    [Trait("Category", "BookingRepository")]
    public async Task GetByIdAsync_WithInvalidBookingId_ThrowsKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new BookingRepository(actContext);
        var bookingId = Guid.NewGuid();
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await repository.GetByIdAsync(bookingId)
        );
    }

    [Fact]
    [Trait("Category", "BookingRepository")]
    public async Task DeleteByIdAsync_WithValidBookingId_DeletesBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var arrangeRepository = new BookingRepository(arrangeContext);
        var actRepository = new BookingRepository(actContext);
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        arrangeContext.Events.Add(new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 10
        });
        arrangeContext.Users.Add(new User
        {
            Id = userId,
            Login = "testuser",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        });
        await arrangeContext.SaveChangesAsync();
        var createdBooking = await arrangeRepository.CreateAsync(eventId, userId);
        // Act
        await actRepository.DeleteByIdAsync(createdBooking.Id);
        // Assert
        var deletedBooking = await actContext.Bookings.FindAsync(createdBooking.Id);
        Assert.Null(deletedBooking);
    }

    [Fact]
    [Trait("Category", "BookingRepository")]
    public async Task DeleteByIdAsync_WithInvalidBookingId_ThrowsKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new BookingRepository(actContext);
        var bookingId = Guid.NewGuid();
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await repository.DeleteByIdAsync(bookingId);
        });
    }

    [Fact]
    [Trait("Category", "BookingRepository")]
    public async Task GetBookingByEventIdAsync_ReturnsBookingsForEvent()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new BookingRepository(actContext);
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var event1 = new Event
        {
            Id = eventId1,
            Title = "Test Event 1",
            Description = "Test Description 1",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 10
        };
        var event2 = new Event
        {
            Id = eventId2,
            Title = "Test Event 2",
            Description = "Test Description 2",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 20
        };
        arrangeContext.Events.AddRange(event1, event2);

        arrangeContext.Users.Add(new User
        { 
            Id = userId,
            Login = "testuser",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        });

        await arrangeContext.SaveChangesAsync();

        var bookingRepository = new BookingRepository(actContext);

        // Act
        await bookingRepository.CreateAsync(eventId1, userId);
        await bookingRepository.CreateAsync(eventId1, userId);
        await bookingRepository.CreateAsync(eventId2, userId);
        await bookingRepository.CreateAsync(eventId2, userId);
        await bookingRepository.CreateAsync(eventId2, userId);

        var bookingsForEvent1 = await bookingRepository.GetBookingsByEventIdAsync(eventId1);
        var bookingsForEvent2 = await bookingRepository.GetBookingsByEventIdAsync(eventId2);

        // Assert
        Assert.Equal(2, bookingsForEvent1.Count());
        Assert.All(bookingsForEvent1, b => Assert.Equal(eventId1, b.EventId));

        Assert.Equal(3, bookingsForEvent2.Count());
        Assert.All(bookingsForEvent2, b => Assert.Equal(eventId2, b.EventId));
    }

    [Fact]
    [Trait("Category", "BookingRepository")]
    public async Task GetAll_ReturnsAllBookings()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new BookingRepository(actContext);
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        arrangeContext.Events.Add(new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 10
        });
        arrangeContext.Users.Add(new User
        {
            Id = userId,
            Login = "testuser",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        });
        await arrangeContext.SaveChangesAsync();

        var booking1 = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now.ToUniversalTime()
        };
        var booking2 = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now.ToUniversalTime()
        };
        arrangeContext.Bookings.AddRange(booking1, booking2);
        await arrangeContext.SaveChangesAsync();
        // Act
        var allBookings = repository.GetAll().ToList();
        // Assert
        Assert.Equal(2, allBookings.Count);
        Assert.Contains(allBookings, b => b.Id == booking1.Id);
        Assert.Contains(allBookings, b => b.Id == booking2.Id);
    }
}
