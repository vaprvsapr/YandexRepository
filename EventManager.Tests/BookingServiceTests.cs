using Moq;
using EventManager.Interfaces;
using EventManager.Models.Bookings;
using EventManager.Models.Events;
using EventManager.Services;
using EventManager.Data;
using Xunit.Internal;

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
            EndAt = DateTime.Now.AddHours(1)
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
            EndAt = DateTime.Now.AddHours(1)
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
            EndAt = DateTime.Now.AddHours(1)
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
            EndAt = DateTime.Now.AddHours(1)
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
            EndAt = DateTime.Now.AddHours(1)
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
            EndAt = DateTime.Now.AddHours(1)
        };
        new EventRepository().Add(newEvent);
        new EventRepository().Delete(newEvent);
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        var BookingService = new BookingService(mockBookingRepository.Object, new EventRepository());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => BookingService.CreateBookingAsync(eventId));
        mockBookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Never);
    }
}
