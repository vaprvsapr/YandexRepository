using Moq;
using EventManager.Models.Bookings;
using EventManager.Models.Events;
using EventManager.Services;
using EventManager.ExceptionHandling;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using EventManager.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Tests;

public class BookingServiceTests
{
    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CreateBookingAsync_WithValidEventId_ReturnsBookingDto()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        };
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Events.Add(newEvent);
        context.SaveChanges();
        var bookingService = new BookingService(context, mockLogger.Object);

        // Act
        var result = await bookingService.CreateBookingAsync(eventId);
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
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        };
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Events.Add(newEvent);
        context.SaveChanges();
        var bookingService = new BookingService(context, mockLogger.Object);
        // Act
        var result1 = await bookingService.CreateBookingAsync(eventId);
        var result2 = await bookingService.CreateBookingAsync(eventId);
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
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        };
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = new BookingService(context, mockLogger.Object);

        // Act, Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task GetBookingByIdAsync_WithValidBookingId_ReturnsBookingDto()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        };
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Events.Add(newEvent);
        context.SaveChanges();
        var bookingService = new BookingService(context, mockLogger.Object);
        // Act
        var booking = await bookingService.CreateBookingAsync(eventId);
        var result = await bookingService.GetBookingByIdAsync(booking.Id);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task GetBookingByIdAsync_WithInvalidBookingId_ReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = new BookingService(context, mockLogger.Object);
        // Act, Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => bookingService.GetBookingByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task GetBookingsByEventIdAsync_WhenNoBookingsForEvent_ReturnsEmptyList()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        };
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Events.Add(newEvent);
        context.SaveChanges();
        var bookingService = new BookingService(context, mockLogger.Object);
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
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        };
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Events.Add(newEvent);
        context.SaveChanges();
        var bookingService = new BookingService(context, mockLogger.Object);
        // Act
        await bookingService.CreateBookingAsync(eventId);
        await bookingService.CreateBookingAsync(eventId);
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
        var bookingService = new BookingService(context, mockLogger.Object);
        // Act
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => bookingService.GetBookingsByEventIdAsync(Guid.NewGuid()));
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CreateBookingAsync_WhenEventWasDeleted_ThrowsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        };
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Events.Add(newEvent);
        context.SaveChanges();
        context.Events.Remove(newEvent);
        context.SaveChanges();
        var bookingService = new BookingService(context, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CreateBookingAsync_WhenNotEnoughAvailableSeats_ThrowsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 0
        };
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Events.Add(newEvent);
        context.SaveChanges();
        var bookingService = new BookingService(context, mockLogger.Object);
        
        // Act, Assert
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CreateBookingAsync_AddingValidBooking_DecreasesAvailableSeats()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 3
        };
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Events.Add(newEvent);
        context.SaveChanges();
        var bookingService = new BookingService(context, mockLogger.Object);
        // Act 1
        await bookingService.CreateBookingAsync(eventId);
        // Assert 1
        Assert.Equal(2, newEvent.AvailableSeats);
        // Act 2
        await bookingService.CreateBookingAsync(eventId);
        // Assert 2
        Assert.Equal(1, newEvent.AvailableSeats);
        // Act 3
        await bookingService.CreateBookingAsync(eventId);
        // Assert 3
        Assert.Equal(0, newEvent.AvailableSeats);
        // Act 4 - trying to create booking when no seats are available
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => bookingService.CreateBookingAsync(eventId));
        Assert.Equal(3, context.Bookings.Count());
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CompetitionTest_CreateMultipleBookingsForSameEvent_EnsuresUniqueBookingIdsAndCorrectSeatCount()
    {
        // Arrange
        int totalSeats = 1000;
        var eventId = Guid.NewGuid();
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = totalSeats
        };
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Events.Add(newEvent);
        context.SaveChanges();
        var bookingService = new BookingService(context, mockLogger.Object);
        // Act
        var tasks = Enumerable.Range(0, totalSeats).Select(async i => await bookingService.CreateBookingAsync(eventId));
        await Task.WhenAll(tasks);
        // Assert
        Assert.Equal(0, newEvent.AvailableSeats);
        Assert.Equal(totalSeats, context.Bookings.Select(b => b.Id).Distinct().Count());
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task OverbookingTest_AttemptToOverbook_ThrowsNoAvailableSeatsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 5
        };
        var mockLogger = new Mock<ILogger<BookingService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Events.Add(newEvent);
        context.SaveChanges();
        var bookingService = new BookingService(context, mockLogger.Object);
        // Act

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => bookingService.CreateBookingAsync(eventId))
            .ToArray();

        await Task.WhenAll(tasks.Select(t => t.ContinueWith(_ => { })));

        // Assert
        var succeeded = tasks.Count(t => t.Status == TaskStatus.RanToCompletion);
        var failed = tasks.Count(t =>
            t.IsFaulted &&
            (t.Exception?.InnerException is NoAvailableSeatsException ||
             t.Exception?.Flatten().InnerExceptions.Any(e => e is NoAvailableSeatsException) == true));

        Assert.Equal(5, succeeded);
        Assert.Equal(15, failed);
    }
}
