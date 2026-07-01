using EventManager.Domain.Models;
using EventManager.Application.Services;
using EventManager.Application.Dto;
using EventManager.Application.Queries;
using EventManager.Application.Repositories;
using EventManager.Infrastructure.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EventManager.Tests.Unit;

public class EventServiceTests
{
    private readonly List<Event> _events =
    [
        new Event { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af01"), Title = "First", StartAt = new DateTime().AddDays(0).ToUniversalTime(), EndAt = new DateTime().AddDays(1).ToUniversalTime(), TotalSeats = 10 },
        new Event { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af02"), Title = "Second", StartAt = new DateTime().AddDays(1).ToUniversalTime(), EndAt = new DateTime().AddDays(2).ToUniversalTime(), TotalSeats = 10 },
        new Event { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af03"), Title = "Third", StartAt = new DateTime().AddDays(2).ToUniversalTime(), EndAt = new DateTime().AddDays(3).ToUniversalTime(), TotalSeats = 10 },
        new Event { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af04"), Title = "Fourth", StartAt = new DateTime().AddDays(3).ToUniversalTime(), EndAt = new DateTime().AddDays(4).ToUniversalTime(), TotalSeats = 10 },
        new Event { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af05"), Title = "Fifth", StartAt = new DateTime().AddDays(4).ToUniversalTime(), EndAt = new DateTime().AddDays(5).ToUniversalTime(), TotalSeats = 10 },
        new Event { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af06"), Title = "Sixth", StartAt = new DateTime().AddDays(5).ToUniversalTime(), EndAt = new DateTime().AddDays(6).ToUniversalTime(), TotalSeats = 10 },
        new Event { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af07"), Title = "Seventh", StartAt = new DateTime().AddDays(6).ToUniversalTime(), EndAt = new DateTime().AddDays(7).ToUniversalTime(), TotalSeats = 10},
        new Event { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af08"), Title = "Eighth", StartAt = new DateTime().AddDays(7).ToUniversalTime(), EndAt = new DateTime().AddDays(8).ToUniversalTime(), TotalSeats = 10 },
        new Event { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af09"), Title = "Ninth", StartAt = new DateTime().AddDays(8).ToUniversalTime(), EndAt = new DateTime().AddDays(9).ToUniversalTime(), TotalSeats = 10 },
        new Event { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af10"), Title = "Tenth", StartAt = new DateTime().AddDays(9).ToUniversalTime(), EndAt = new DateTime().AddDays(10).ToUniversalTime(), TotalSeats = 10 },
        new Event { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af11"), Title = "Eleventh", StartAt = new DateTime().AddDays(10).ToUniversalTime(), EndAt = new DateTime().AddDays(11).ToUniversalTime(), TotalSeats = 10 }
    ];

    [Fact]
    [Trait("Category", "EventService")]
    public async Task CreateEvent_CreatingValidEvent_ReturnsTrue()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        EventCreateDto newEvent = new()
        {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        };
        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        // Act
        var createdEvent = await eventService.CreateEvent(newEvent);

        // Act

        // Assert
        Assert.Equal(1, context.Events.Count());
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task GetAllEvents_WithoutFiltration_ReturnsEvents()
    {
        // Arrange

        GetEventQuery query = new();

        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        context.Events.AddRange(_events);
        context.SaveChanges();

        // Act
        var result = await eventService.GetAllEvents(query);

        // Assert
        Assert.Equal(_events.Count, result.TotalCount);
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task GetAllEvents_WithTitleFiltration_ReturnsEvents()
    {
        // Arrange
        GetEventQuery query = new() { Title = "th" };

        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        context.Events.AddRange(_events);
        context.SaveChanges();

        // Act
        var result = await eventService.GetAllEvents(query);

        // Assert
        Assert.Equal(9, result.TotalCount);
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task GetAllEvents_WithDateFiltration_ReturnsEvents()
    {
        // Arrange
        GetEventQuery fromQuery = new() { From = new DateTime().AddDays(5).ToUniversalTime() };
        GetEventQuery toQuery = new() { To = new DateTime().AddDays(5).ToUniversalTime() };
        GetEventQuery fromToQuery = new() { From = new DateTime().AddDays(4).ToUniversalTime(), To = new DateTime().AddDays(6).ToUniversalTime() };

        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        context.Events.AddRange(_events);
        context.SaveChanges();

        // Act
        var fromQueryResult = await eventService.GetAllEvents(fromQuery);
        var toQueryResult = await eventService.GetAllEvents(toQuery);
        var fromToQueryResult = await eventService.GetAllEvents(fromToQuery);

        // Assert
        Assert.Equal(6, fromQueryResult.TotalCount);
        Assert.Equal(5, toQueryResult.TotalCount);
        Assert.Equal(2, fromToQueryResult.TotalCount);
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task GetAllEvents_WithCombinedFiltration_ReturnsEvents()
    {
        // Arrange
        GetEventQuery query = new() 
        { 
            Title = "I", 
            From = new DateTime().AddDays(4).ToUniversalTime(), 
            To = new DateTime().AddDays(8).ToUniversalTime() 
        };

        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        context.Events.AddRange(_events);
        context.SaveChanges();

        // Act
        var result = await eventService.GetAllEvents(query);

        // Assert
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task GetAllEvents_Pagination_ReturnsEvents()
    {
        // Arrange
        GetEventQuery query = new() { Page = 4, PageSize = 3 };

        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        context.Events.AddRange(_events);
        context.SaveChanges();
        // Act
        var result = await eventService.GetAllEvents(query);
        // Assert
        Assert.Equal(11, result.TotalCount);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(4, result.Page);
        Assert.Equal(2, result.Events?.Count() ?? 0);
        Assert.Equal(_events[^2].Id, result.Events?.ToList()[0].Id);
        Assert.Equal(_events[^1].Id, result.Events?.ToList()[1].Id);
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task GetEvent_ExistingId_ReturnsEvent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        context.Events.AddRange(_events);
        context.SaveChanges();
        // Act
        var result = await eventService.GetEvent(_events[0].Id);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(_events[0].Id, result.Id);
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task GetEvent_WithInvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options 
            => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        context.Events.AddRange(_events);
        context.SaveChanges();
        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await eventService.GetEvent(Guid.NewGuid()));
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task UpdateEvent_ExistingId_UpdatesEvent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        context.Events.AddRange(_events);
        context.SaveChanges();
        // Act
        await eventService.UpdateEvent(_events[0].Id,
            new EventUpdateDto
            {
                Title = "Updated Event",
                StartAt = new DateTime(0),
                EndAt = new DateTime(1)
            });
        // Assert
        Assert.Equal("Updated Event", context.Events.First(e => e.Id == _events[0].Id).Title);
        Assert.Equal(new DateTime(0), context.Events.First(e => e.Id == _events[0].Id).StartAt);
        Assert.Equal(new DateTime(1), context.Events.First(e => e.Id == _events[0].Id).EndAt);
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task UpdateEvent_NonExistingId_ThrowsException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        // Act

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await eventService.UpdateEvent(Guid.NewGuid(),
                new EventUpdateDto
                {
                    Title = "Updated Event",
                    StartAt = new DateTime(0),
                    EndAt = new DateTime(1)
            }));
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task DeleteEvent_ExistingId_ReturnsTrue()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        context.Events.AddRange(_events);
        context.SaveChanges();
        // Act
        await eventService.DeleteEvent(_events[0].Id);
        // Assert
        Assert.Equal(_events.Count - 1, context.Events.Count());
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task DeleteEvent_NonExistingId_ThrowsException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EventService>>();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEventRepository eventRepository = new EventRepository(context);
        var eventService = new EventService(eventRepository, mockLogger.Object);
        context.Events.AddRange(_events);
        context.SaveChanges();
        // Act

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await eventService.DeleteEvent(Guid.NewGuid()));
    }
}
