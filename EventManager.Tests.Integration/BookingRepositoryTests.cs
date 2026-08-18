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
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now.ToUniversalTime()
        };
        await repository.CreateAsync(booking);
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
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now.ToUniversalTime()
        };
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await repository.CreateAsync(booking));
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
    public async Task GetByIdAsync_WithInvalidBookingId_ReturnsNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new BookingRepository(actContext);
        var bookingId = Guid.NewGuid();
        // Act & Assert
        var nullBooking = await repository.GetByIdAsync(bookingId);
        Assert.Null(nullBooking);
    }

    [Fact]
    [Trait("Category", "BookingRepository")]
    public async Task DeleteAsync_WithValidBooking_DeletesBooking()
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

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now.ToUniversalTime()
        };
        await arrangeRepository.CreateAsync(booking);
        // Act
        await actRepository.DeleteAsync(booking);
        // Assert
        var deletedBooking = await actContext.Bookings.FindAsync(booking.Id);
        Assert.Null(deletedBooking);
    }

    [Fact]
    [Trait("Category", "BookingRepository")]
    public async Task DeleteAsync_WithInvalidBooking_ThrowsKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new BookingRepository(actContext);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now.ToUniversalTime()
        };
        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await repository.DeleteAsync(booking);
        });
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
