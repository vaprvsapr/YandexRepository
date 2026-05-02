using Moq;
using EventManager.Interfaces;
using EventManager.Models.Bookings;
using EventManager.Models.Events;
using EventManager.Services;

namespace EventManager.Tests;

public class BookingServiceTests
{
    [Fact]
    [Trait("Category", "BookingService")]
    public void CreateBookingAsync_WithValidEventId_ReturnsBookingDto()
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
        var result = bookingService.CreateBookingAsync(eventId);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
        mockEventRepository.Verify(repo => repo.GetById(eventId), Times.Once);
        mockBookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public void CreateBookingAsync_CreatingMultipleBookings_AssignesUniqueBookingIds()
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
        var result1 = bookingService.CreateBookingAsync(eventId);
        var result2 = bookingService.CreateBookingAsync(eventId);
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
    public void CreateBookingAsync_WithInvalidEventId_ThrowsException()
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
        Assert.Throws<KeyNotFoundException>(() => bookingService.CreateBookingAsync(eventId));
        mockEventRepository.Verify(repo => repo.GetById(eventId), Times.Once);
        mockBookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public void GetBookingByIdAsync_WithValidBookingId_ReturnsBookingDto()
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
        var result = bookingService.GetBookingByIdAsync(bookingId);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookingId, result.Id);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
        mockBookingRepository.Verify(repo => repo.GetById(bookingId), Times.Once);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public void GetBookingByIdAsync_WithInvalidBookingId_ReturnsNull()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var mockEventRepository = new Mock<IRepository<Event>>();
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        mockBookingRepository.Setup(repo => repo.GetById(It.IsAny<Guid>())).Returns((Guid id) => null);
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);
        // Act, Assert
        Assert.Throws<KeyNotFoundException>(() => bookingService.GetBookingByIdAsync(bookingId));
        mockBookingRepository.Verify(repo => repo.GetById(bookingId), Times.Once);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public void GetBookingsByEventIdAsync_WhenNoBookingsForEvent_ReturnsEmptyList()
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
        mockBookingRepository.Setup(repo => repo.GetAll()).Returns(new List<Booking>());
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);
        // Act
        var result = bookingService.GetBookingsByEventIdAsync(eventId);
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        mockEventRepository.Verify(repo => repo.GetById(eventId), Times.Once);
        mockBookingRepository.Verify(repo => repo.GetAll(), Times.Once);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public void GetBookingsByEventIdAsync_WithMultipleBookingsForEvent_ReturnsCorrectList()
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
            new Booking { Id = Guid.NewGuid(), EventId = eventId, Status = BookingStatus.Pending, CreatedAt = DateTime.Now, ProcessedAt = DateTime.Now },
            new Booking { Id = Guid.NewGuid(), EventId = eventId, Status = BookingStatus.Confirmed, CreatedAt = DateTime.Now, ProcessedAt = DateTime.Now },
            new Booking { Id = Guid.NewGuid(), EventId = Guid.NewGuid(), Status = BookingStatus.Pending, CreatedAt = DateTime.Now, ProcessedAt = DateTime.Now }
        };
        var mockEventRepository = new Mock<IRepository<Event>>();
        mockEventRepository.Setup(repo => repo.GetById(eventId)).Returns(newEvent);
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        mockBookingRepository.Setup(repo => repo.GetAll()).Returns(bookings);
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);
        // Act
        var result = bookingService.GetBookingsByEventIdAsync(eventId);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.Equal(eventId, dto.EventId));
        mockEventRepository.Verify(repo => repo.GetById(eventId), Times.Once);
        mockBookingRepository.Verify(repo => repo.GetAll(), Times.Once);
    }

    [Fact]
    [Trait("Category", "BookingService")]
    public void GetBookingsByEventIdAsync_WithInvalidEventId_ThrowsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var mockEventRepository = new Mock<IRepository<Event>>();
        mockEventRepository.Setup(repo => repo.GetById(eventId)).Returns((Event)null!);
        var mockBookingRepository = new Mock<IRepository<Booking>>();
        var bookingService = new BookingService(mockBookingRepository.Object, mockEventRepository.Object);
        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => bookingService.GetBookingsByEventIdAsync(eventId));
        mockEventRepository.Verify(repo => repo.GetById(eventId), Times.Once);
        mockBookingRepository.Verify(repo => repo.GetAll(), Times.Never);
    }
}
