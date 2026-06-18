using EventManager.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;

namespace EventManager.IntegrationTests;

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
        var repository = new DataAccess.Repositories.BookingRepository(actContext);
        var eventId = Guid.NewGuid();
        arrangeContext.Events.Add(new Models.Events.Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 10
        });
        await arrangeContext.SaveChangesAsync();
        // Act
        var booking = await repository.CreateAsync(eventId);
        // Assert
        Assert.NotNull(booking);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(Models.Bookings.BookingStatus.Pending, booking.Status);
    }

    [Fact]
    [Trait("Category", "BookingRepoisitory")]
    public async Task CreateAsync_WithInvalidEventId_ThrowsKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new DataAccess.Repositories.BookingRepository(actContext);
        var eventId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await repository.CreateAsync(eventId));
    }

    [Fact]
    [Trait("Category", "BookingRepository")]
    public async Task GetByIdAsync_WithValidBookingId_ReturnBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new DataAccess.Repositories.BookingRepository(actContext);
        var eventId = Guid.NewGuid();
        arrangeContext.Events.Add(new Models.Events.Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 10
        });
        await arrangeContext.SaveChangesAsync();
        var booking = new Models.Bookings.Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = Models.Bookings.BookingStatus.Pending,
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
        var repository = new DataAccess.Repositories.BookingRepository(actContext);
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
        var arrangeRepository = new DataAccess.Repositories.BookingRepository(arrangeContext);
        var actRepository = new DataAccess.Repositories.BookingRepository(actContext);
        var eventId = Guid.NewGuid();
        arrangeContext.Events.Add(new Models.Events.Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 10
        });
        await arrangeContext.SaveChangesAsync();
        var createdBooking = await arrangeRepository.CreateAsync(eventId);
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
        var repository = new DataAccess.Repositories.BookingRepository(actContext);
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
        var repository = new DataAccess.Repositories.BookingRepository(actContext);
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();

        var event1 = new Models.Events.Event
        {
            Id = eventId1,
            Title = "Test Event 1",
            Description = "Test Description 1",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 10
        };

        var event2 = new Models.Events.Event
        {
            Id = eventId2,
            Title = "Test Event 2",
            Description = "Test Description 2",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 20
        };

        arrangeContext.Events.AddRange(event1, event2);
        await arrangeContext.SaveChangesAsync();

        var bookingRepository = new DataAccess.Repositories.BookingRepository(actContext);

        // Act
        await bookingRepository.CreateAsync(eventId1);
        await bookingRepository.CreateAsync(eventId1);
        await bookingRepository.CreateAsync(eventId2);
        await bookingRepository.CreateAsync(eventId2);
        await bookingRepository.CreateAsync(eventId2);

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
        var repository = new DataAccess.Repositories.BookingRepository(actContext);
        var eventId = Guid.NewGuid();
        arrangeContext.Events.Add(new Models.Events.Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now.ToUniversalTime(),
            EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
            TotalSeats = 10
        });
        await arrangeContext.SaveChangesAsync();
        var booking1 = new Models.Bookings.Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = Models.Bookings.BookingStatus.Pending,
            CreatedAt = DateTime.Now.ToUniversalTime()
        };
        var booking2 = new Models.Bookings.Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = Models.Bookings.BookingStatus.Confirmed,
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
