using EventManager.Application.Repositories;
using EventManager.Application.Services;
using EventManager.Application.Services.Interfaces;
using EventManager.Domain.Exceptions;
using EventManager.Domain.Models;
using EventManager.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventManager.Tests.Unit;

public class BookingServiceTests
{
    private (IBookingService, AppDbContext) ArrangeBookingService(Guid eventId, Guid userId)
    {
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 5
        };

        User newUser = new()
        {
            Id = userId,
            Login = "testuser",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };

        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Events.Add(newEvent);
        context.Users.Add(newUser);
        context.SaveChanges();
        var bookingRepository = new BookingRepository(context);
        var eventRepository = new EventRepository(context);
        var userRepository = new UserRepository(context);
        var bookingService = new BookingService(bookingRepository, eventRepository, userRepository, mockLogger.Object);

        return (bookingService, context);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CreateBookingAsync_WithValidEventId_ReturnsBookingDto()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookingService = ArrangeBookingService(eventId, userId).Item1;

        // Act
        var result = await bookingService.CreateBookingAsync(eventId, userId);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CreateBookingAsync_CreatingMultipleBookings_AssignesUniqueBookingIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookingService = ArrangeBookingService(eventId, userId).Item1;
        // Act
        var result1 = await bookingService.CreateBookingAsync(eventId, userId);
        var result2 = await bookingService.CreateBookingAsync(eventId, userId);
        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotEqual(result1.Id, result2.Id);
        Assert.Equal(eventId, result1.EventId);
        Assert.Equal(BookingStatus.Pending, result1.Status);
        Assert.Equal(eventId, result2.EventId);
        Assert.Equal(BookingStatus.Pending, result2.Status);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CreateBookingAsync_WithInvalidEventId_ThrowsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookingService = ArrangeBookingService(eventId, userId).Item1;

        // Act, Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => bookingService.CreateBookingAsync(Guid.NewGuid(), userId));
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task GetBookingByIdAsync_WithValidBookingId_ReturnsBookingDto()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookingService = ArrangeBookingService(eventId, userId).Item1;
        // Act
        var booking = await bookingService.CreateBookingAsync(eventId, userId);
        var result = await bookingService.GetBookingByIdAsync(booking.Id);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task GetBookingByIdAsync_WithInvalidBookingId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingRepository = new BookingRepository(context);
        var eventRepository = new EventRepository(context);
        var userRepository = new UserRepository(context);
        var bookingService = new BookingService(bookingRepository, eventRepository, userRepository, mockLogger.Object);
        // Act, Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await bookingService.GetBookingByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task GetBookingsByEventIdAsync_WhenNoBookingsForEvent_ReturnsEmptyList()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookingService = ArrangeBookingService(eventId, userId).Item1;
        // Act
        var result = await bookingService.GetBookingsByEventIdAsync(eventId);
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task GetBookingsByEventIdAsync_WithMultipleBookingsForEvent_ReturnsCorrectList()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookingService = ArrangeBookingService(eventId, userId).Item1;
        // Act
        await bookingService.CreateBookingAsync(eventId, userId);
        await bookingService.CreateBookingAsync(eventId, userId);
        var result = await bookingService.GetBookingsByEventIdAsync(eventId);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.Equal(eventId, dto.EventId));
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task GetBookingsByEventIdAsync_WithInvalidEventId_ThrowsException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingRepository = new BookingRepository(context);
        var eventRepository = new EventRepository(context);
        var userRepository = new UserRepository(context);
        var bookingService = new BookingService(bookingRepository, eventRepository, userRepository, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => bookingService.GetBookingsByEventIdAsync(Guid.NewGuid()));
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CreateBookingAsync_WhenEventWasDeleted_ThrowsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var arrangeResults = ArrangeBookingService(eventId, userId);
        var bookingService = arrangeResults.Item1;
        var context = arrangeResults.Item2;

        var newEvent = await context.Events.FindAsync(eventId) ?? throw new KeyNotFoundException("Event not found");
        context.Events.Remove(newEvent);
        context.SaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => bookingService.CreateBookingAsync(eventId, userId));
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CreateBookingAsync_WhenNotEnoughAvailableSeats_ThrowsNoAvailableSeatsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookingService = ArrangeBookingService(eventId, userId).Item1;

        // Act, Assert
        await bookingService.CreateBookingAsync(eventId, userId);
        await bookingService.CreateBookingAsync(eventId, userId);
        await bookingService.CreateBookingAsync(eventId, userId);
        await bookingService.CreateBookingAsync(eventId, userId);
        await bookingService.CreateBookingAsync(eventId, userId);
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => bookingService.CreateBookingAsync(eventId, userId));
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CreateBookingAsync_AddingValidBooking_DecreasesAvailableSeats()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var arrangeResults = ArrangeBookingService(eventId, userId);
        var bookingService = arrangeResults.Item1;
        var context = arrangeResults.Item2;
        var newEvent = await context.Events.FindAsync(eventId) ?? throw new KeyNotFoundException("Event not found");
        // Act 1
        await bookingService.CreateBookingAsync(eventId, userId);
        // Assert 1
        Assert.Equal(4, newEvent.AvailableSeats);
        // Act 2
        await bookingService.CreateBookingAsync(eventId, userId);
        // Assert 2
        Assert.Equal(3, newEvent.AvailableSeats);
        // Act 3
        await bookingService.CreateBookingAsync(eventId, userId);
        // Assert 3
        Assert.Equal(2, newEvent.AvailableSeats);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CompetitionTest_CreateMultipleBookingsForSameEvent_EnsuresUniqueBookingIdsAndCorrectSeatCount()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var arrangeResults = ArrangeBookingService(eventId, userId);
        var bookingService = arrangeResults.Item1;
        var context = arrangeResults.Item2;
        var newEvent = await context.Events.FindAsync(eventId) ?? throw new KeyNotFoundException("Event not found");
        // Act
        var tasks = Enumerable.Range(0, 5).Select(async i => await bookingService.CreateBookingAsync(eventId, userId));
        await Task.WhenAll(tasks);
        // Assert
        Assert.Equal(0, newEvent.AvailableSeats);
        Assert.Equal(5, context.Bookings.Select(b => b.Id).Distinct().Count());
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task OverbookingTest_AttemptToOverbook_ThrowsNoAvailableSeatsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookingService = ArrangeBookingService(eventId, userId).Item1;
        // Act

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => bookingService.CreateBookingAsync(eventId, userId))
            .ToArray();

        await Task.WhenAll(tasks.Select(t => t.ContinueWith(_ => { })));

        // Assert
        var succeeded = tasks.Count(t => t.Status == TaskStatus.RanToCompletion);
        var failed = tasks.Count(t =>
            t.IsFaulted &&
            (t.Exception?.InnerException is NoAvailableSeatsException ||
             t.Exception?.Flatten().InnerExceptions.Any(e => e is NoAvailableSeatsException) == true));

        Assert.Equal(5, succeeded);
        Assert.Equal(5, failed);
    }
}
