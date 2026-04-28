namespace EventManager.Tests;

using EventManager.Interfaces;
using EventManager.Models.Event;
using EventManager.Models.Queries;
using EventManager.Services;
using Moq;

public class EventServiceTests
{
    private readonly List<Event> _events =
    [
        new Event { Id = 1, Title = "First", StartAt = new DateTime(0), EndAt = new DateTime(1) },
        new Event { Id = 2, Title = "Second", StartAt = new DateTime(1), EndAt = new DateTime(2) },
        new Event { Id = 3, Title = "Third", StartAt = new DateTime(2), EndAt = new DateTime(3) },
        new Event { Id = 4, Title = "Fourth", StartAt = new DateTime(3), EndAt = new DateTime(4) },
        new Event { Id = 5, Title = "Fifth", StartAt = new DateTime(4), EndAt = new DateTime(5) },
        new Event { Id = 6, Title = "Sixth", StartAt = new DateTime(5), EndAt = new DateTime(6) },
        new Event { Id = 7, Title = "Seventh", StartAt = new DateTime(6), EndAt = new DateTime(7)},
        new Event { Id = 8, Title = "Eighth", StartAt = new DateTime(7), EndAt = new DateTime(8) },
        new Event { Id = 9, Title = "Ninth", StartAt = new DateTime(8), EndAt = new DateTime(9) },
        new Event { Id = 10, Title = "Tenth", StartAt = new DateTime(9), EndAt = new DateTime(10) },
        new Event { Id = 11, Title = "Eleventh", StartAt = new DateTime(10), EndAt = new DateTime(11) }
    ];

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "CreateEvent")]
    public void CreateEvent_CreatingValidEvent_ReturnsTrue()
    {
        // Arrange
        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.Add(It.IsAny<Event>()));
        mockRepository.Setup(m => m.GetAll()).Returns(_events);
        var eventService = new EventService(mockRepository.Object);

        // Act
        eventService.CreateEvent(
            new EventDto
            {
                Id = 12,
                Title = "Test title",
                StartAt = new DateTime(),
                EndAt = new DateTime().AddHours(1),
            });

        // Assert
        mockRepository.Verify(m => m.Add(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "CreateEvent")]
    public void CreateEvent_CreatingInvalidEvent_ThrowsException()
    {
        // Arrange
        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.GetById(It.IsAny<int>())).Returns((int id) => _events.FirstOrDefault(e => e.Id == id));
        var eventService = new EventService(mockRepository.Object);

        // Act


        // Assert
        Assert.Throws<InvalidOperationException>(() => eventService.CreateEvent(
            new EventDto
            {
                Id = 1,
                Title = "Test title",
                StartAt = new DateTime(),
                EndAt = new DateTime().AddHours(1),
            }));
        mockRepository.Verify(m => m.GetById(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "GetAllEvents")]
    public void GetAllEvents_WithoutFiltration_ReturnsEvents()
    {
        // Arrange

        GetQuery query = new();

        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.GetAll()).Returns(_events);
        var eventService = new EventService(mockRepository.Object);

        // Act
        var result = eventService.GetAllEvents(query);

        // Assert
        Assert.Equal(_events.Count, result.TotalCount);
        mockRepository.Verify(m => m.GetAll(), Times.Once);
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "GetAllEvents")]
    public void GetAllEvents_WithTitleFiltration_ReturnsEvents()
    {
        // Arrange
        GetQuery query = new() { Title = "th" };

        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.GetAll()).Returns(_events);
        var eventService = new EventService(mockRepository.Object);

        // Act
        var result = eventService.GetAllEvents(query);

        // Assert
        Assert.Equal(9, result.TotalCount);
        mockRepository.Verify(m => m.GetAll(), Times.Once);
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "GetAllEvents")]
    public void GetAllEvents_WithDateFiltration_ReturnsEvents()
    {
        // Arrange
        GetQuery fromQuery = new() { From = new DateTime(5)};
        GetQuery toQuery = new() { To = new DateTime(5) };
        GetQuery fromToQuery = new() { From = new DateTime(4), To = new DateTime(6) };

        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.GetAll()).Returns(_events);
        var eventService = new EventService(mockRepository.Object);

        // Act
        var fromQueryResult = eventService.GetAllEvents(fromQuery);
        var toQueryResult = eventService.GetAllEvents(toQuery);
        var fromToQueryResult = eventService.GetAllEvents(fromToQuery);

        // Assert
        Assert.Equal(6, fromQueryResult.TotalCount);
        Assert.Equal(5, toQueryResult.TotalCount);
        Assert.Equal(2, fromToQueryResult.TotalCount);
        mockRepository.Verify(m => m.GetAll(), Times.Exactly(3));
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "GetAllEvents")]
    public void GetAllEvents_WithCombinedFiltration_ReturnsEvents()
    {
        // Arrange
        GetQuery query = new() { Title = "I", From = new DateTime(4), To = new DateTime(8) };

        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.GetAll()).Returns(_events);
        var eventService = new EventService(mockRepository.Object);

        // Act
        var result = eventService.GetAllEvents(query);

        // Assert
        Assert.Equal(3, result.TotalCount);
        mockRepository.Verify(m => m.GetAll(), Times.Once);
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "GetAllEvents")]
    public void GetAllEvents_Pagination_ReturnsEvents()
    {
        // Arrange
        GetQuery query = new() { Page = 4, PageSize = 3 };
        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.GetAll()).Returns(_events);
        var eventService = new EventService(mockRepository.Object);
        // Act
        var result = eventService.GetAllEvents(query);
        // Assert
        Assert.Equal(11, result.TotalCount);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(4, result.Page);
        Assert.Equal(2, result.Events?.Count() ?? 0);
        Assert.Equal(_events[^2].Id, result.Events?.ToList()[0].Id);
        Assert.Equal(_events[^1].Id, result.Events?.ToList()[1].Id);
        mockRepository.Verify(m => m.GetAll(), Times.Once);
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "GetEvent")]
    public void GetEvent_ExistingId_ReturnsEvent()
    {
        // Arrange
        int existingId = 1;
        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.GetById(It.IsAny<int>())).Returns(_events.FirstOrDefault(e => e.Id == existingId));
        var eventService = new EventService(mockRepository.Object);
        // Act
        var result = eventService.GetEvent(existingId);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingId, result.Id);
        mockRepository.Verify(m => m.GetById(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "GetEvent")]
    public void GetEvent_NonExistingId_ReturnsNull()
    {
        // Arrange
        int nonExistingId = 999;
        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.GetById(It.IsAny<int>())).Returns((int id) => _events.FirstOrDefault(e => e.Id == id));
        var eventService = new EventService(mockRepository.Object);
        // Act
        
        // Assert
        Assert.Throws<KeyNotFoundException>(() => eventService.GetEvent(nonExistingId));
        mockRepository.Verify(m => m.GetById(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "UpdateEvent")]
    public void UpdateEvent_ExistingId_UpdatesEvent()
    {
        // Arrange
        int existingId = 1;
        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.Update(existingId, It.IsAny<Event>()));
        mockRepository.Setup(m => m.GetById(It.IsAny<int>())).Returns(_events.FirstOrDefault(e => e.Id == existingId));
        var eventService = new EventService(mockRepository.Object);
        // Act
        eventService.UpdateEvent(existingId, 
            new EventDto 
            {  
                Id = existingId,
                Title = "Updated Event",
                StartAt = new DateTime(0), 
                EndAt = new DateTime(1) 
            });
        // Assert
        mockRepository.Verify(m => m.Update(existingId, It.IsAny<Event>()), Times.Once);
        mockRepository.Verify(m => m.GetById(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "UpdateEvent")]
    public void UpdateEvent_NonExistingId_ThrowsException()
    {
        // Arrange
        int nonExistingId = 999;
        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.GetById(It.IsAny<int>())).Returns((int id) => _events.FirstOrDefault(e => e.Id == id));
        var eventService = new EventService(mockRepository.Object);
        // Act

        // Assert
        Assert.Throws<KeyNotFoundException>(() => eventService.UpdateEvent(nonExistingId,
            new EventDto
            {
                Id = nonExistingId,
                Title = "Updated Event",
                StartAt = new DateTime(0),
                EndAt = new DateTime(1)
            }));
        mockRepository.Verify(m => m.GetById(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "DeleteEvent")]
    public void DeleteEvent_ExistingId_ReturnsTrue()
    {
        // Arrange
        int existingId = 1;
        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.Delete(It.IsAny<Event>()));
        mockRepository.Setup(m => m.GetById(It.IsAny<int>())).Returns((int id) => _events.FirstOrDefault(e => e.Id == id));
        var eventService = new EventService(mockRepository.Object);
        // Act
        eventService.DeleteEvent(existingId);
        // Assert
        mockRepository.Verify(m => m.Delete(It.IsAny<Event>()), Times.Once);
        mockRepository.Verify(m => m.GetById(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "EventService")]
    [Trait("Subcategory", "DeleteEvent")]
    public void DeleteEvent_NonExistingId_ThrowsException()
    {
        // Arrange
        int nonExistingId = 999;
        var mockRepository = new Mock<IEventRepository>();
        mockRepository.Setup(m => m.GetById(It.IsAny<int>())).Returns((int id) => _events.FirstOrDefault(e => e.Id == id));
        var eventService = new EventService(mockRepository.Object);
        // Act

        // Assert
        Assert.Throws<KeyNotFoundException>(() => eventService.DeleteEvent(nonExistingId));
        mockRepository.Verify(m => m.GetById(It.IsAny<int>()), Times.Once);
    }
}
