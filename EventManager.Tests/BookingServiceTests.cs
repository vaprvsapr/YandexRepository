using Moq;
using EventManager.Interfaces;
using EventManager.Models.Bookings;
using EventManager.Models.Events;
using EventManager.Services;
using EventManager.Data;
using EventManager.ExceptionHandling;

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
        var mockEventRepository = new Mock<IRepository<Event>>();
        mockEventRepository.Setup(repo => repo.GetById(eventId)).Returns(newEvent);
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);
        // Act
        var result = await bookingService.CreateBookingAsync(eventId);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
        mockEventRepository.Verify(repo => repo.GetById(eventId), Times.Once);
        mockBookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Once);
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
        var mockEventRepository = new Mock<IRepository<Event>>();
        mockEventRepository.Setup(repo => repo.GetById(eventId)).Returns(newEvent);
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);
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
        mockEventRepository.Verify(repo => repo.GetById(eventId), Times.Exactly(2));
        mockBookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Exactly(2));
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
        var mockEventRepository = new Mock<IRepository<Event>>();
        mockEventRepository.Setup(repo => repo.GetById(It.IsAny<Guid>())).Returns((Guid id) => null);
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);

        // Act, Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => bookingService.CreateBookingAsync(eventId));
        mockEventRepository.Verify(repo => repo.GetById(eventId), Times.Once);
        mockBookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task GetBookingByIdAsync_WithValidBookingId_ReturnsBookingDto()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        Booking existingBooking = new()
        {
            Id = bookingId,
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
            ProcessedAt = DateTime.Now
        };
        var mockEventRepository = new Mock<IRepository<Event>>();
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        mockBookingRepository.Setup(repo => repo.GetById(bookingId)).Returns(existingBooking);
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);
        // Act
        var result = await bookingService.GetBookingByIdAsync(bookingId);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookingId, result.Id);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
        mockBookingRepository.Verify(repo => repo.GetById(bookingId), Times.Once);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task GetBookingByIdAsync_WithInvalidBookingId_ReturnsNull()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var mockEventRepository = new Mock<IRepository<Event>>();
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        mockBookingRepository.Setup(repo => repo.GetById(It.IsAny<Guid>())).Returns((Guid id) => null);
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);
        // Act, Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => bookingService.GetBookingByIdAsync(bookingId));
        mockBookingRepository.Verify(repo => repo.GetById(bookingId), Times.Once);
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
        var mockEventRepository = new Mock<IRepository<Event>>();
        mockEventRepository.Setup(repo => repo.GetById(eventId)).Returns(newEvent);
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        mockBookingRepository.Setup(repo => repo.GetAll()).Returns([]);
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);
        // Act
        var result = await bookingService.GetBookingsByEventIdAsync(eventId);
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        mockEventRepository.Verify(repo => repo.GetById(eventId), Times.Once);
        mockBookingRepository.Verify(repo => repo.GetAll(), Times.Once);
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
        var bookings = new List<Booking>
        {
            new() { Id = Guid.NewGuid(), EventId = eventId, Status = BookingStatus.Pending, CreatedAt = DateTime.Now, ProcessedAt = DateTime.Now },
            new() { Id = Guid.NewGuid(), EventId = eventId, Status = BookingStatus.Confirmed, CreatedAt = DateTime.Now, ProcessedAt = DateTime.Now },
            new() { Id = Guid.NewGuid(), EventId = Guid.NewGuid(), Status = BookingStatus.Pending, CreatedAt = DateTime.Now, ProcessedAt = DateTime.Now }
        };
        var mockEventRepository = new Mock<IRepository<Event>>();
        mockEventRepository.Setup(repo => repo.GetById(eventId)).Returns(newEvent);
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        mockBookingRepository.Setup(repo => repo.GetAll()).Returns(bookings);
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);
        // Act
        var result = await bookingService.GetBookingsByEventIdAsync(eventId);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.Equal(eventId, dto.EventId));
        mockEventRepository.Verify(repo => repo.GetById(eventId), Times.Once);
        mockBookingRepository.Verify(repo => repo.GetAll(), Times.Once);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task GetBookingsByEventIdAsync_WithInvalidEventId_ThrowsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var mockEventRepository = new Mock<IRepository<Event>>();
        mockEventRepository.Setup(repo => repo.GetById(eventId)).Returns((Event)null!);
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => bookingService.GetBookingsByEventIdAsync(eventId));
        mockEventRepository.Verify(repo => repo.GetById(eventId), Times.Once);
        mockBookingRepository.Verify(repo => repo.GetAll(), Times.Never);
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
        EventRepository eventRepository = new();
        eventRepository.Add(newEvent);
        eventRepository.Delete(newEvent);
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        var BookingService = new BookingService(mockBookingRepository.Object, eventRepository);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => BookingService.CreateBookingAsync(eventId));
        mockBookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Never);
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
        EventRepository eventRepository = new();
        eventRepository.Add(newEvent);
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        var BookingService = new BookingService(mockBookingRepository.Object, eventRepository);

        // Act

        // Assert
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => BookingService.CreateBookingAsync(eventId));
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
        EventRepository eventRepository = new();
        eventRepository.Add(newEvent);
        BookingRepository bookingRepository = new();
        var BookingService = new BookingService(bookingRepository, eventRepository);
        // Act 1
        await BookingService.CreateBookingAsync(eventId);
        // Assert 1
        Assert.Equal(2, newEvent.AvailableSeats);
        // Act 2
        await BookingService.CreateBookingAsync(eventId);
        // Assert 2
        Assert.Equal(1, newEvent.AvailableSeats);
        // Act 3
        await BookingService.CreateBookingAsync(eventId);
        // Assert 3
        Assert.Equal(0, newEvent.AvailableSeats);
        // Act 4 - trying to create booking when no seats are available
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => BookingService.CreateBookingAsync(eventId));
        Assert.Equal(3, bookingRepository.GetAll().Select(b => b.Id).Distinct().Count());
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public async Task CompetitionTest_CreateMultipleBookingsForSameEvent_EnsuresUniqueBookingIdsAndCorrectSeatCount()
    {
        // Arrange
        int numberOfSeats = 10;
        var eventId = Guid.NewGuid();
        Event newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = numberOfSeats
        };
        EventRepository eventRepository = new();
        eventRepository.Add(newEvent);
        BookingRepository bookingRepository = new();
        var BookingService = new BookingService(bookingRepository, eventRepository);
        // Act
        var tasks = Enumerable.Range(0, numberOfSeats).Select(async i => await BookingService.CreateBookingAsync(eventId));
        await Task.WhenAll(tasks);
        // Assert
        Assert.Equal(0, newEvent.AvailableSeats);
        Assert.Equal(numberOfSeats, bookingRepository.GetAll().Select(b => b.Id).Distinct().Count());
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
        EventRepository eventRepository = new();
        eventRepository.Add(newEvent);
        BookingRepository bookingRepository = new();
        var BookingService = new BookingService(bookingRepository, eventRepository);
        // Act

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => BookingService.CreateBookingAsync(eventId))
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
