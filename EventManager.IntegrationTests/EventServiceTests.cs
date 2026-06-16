using EventManager.DataAccess;
using EventManager.DataAccess.Repositories;
using EventManager.Models.Events;
using EventManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;
using EventManager.Models.Queries;

namespace EventManager.IntegrationTests;

public class EventServiceTests : PostgresTest
{
    public EventServiceTests(PostgresFixture postgresFixture) : base(postgresFixture)
    { }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task GetAllEvents_WithTitleQuery_ReturnsEventsMatchingTitle()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();

        var eventRepository = new EventRepository(actContext);
        var mockLogger = new Mock<ILogger<EventService>>();
        var eventService = new EventService(eventRepository, mockLogger.Object);

        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Music Concert",
            Description = "A live music concert.",
            StartAt = new DateTime().ToUniversalTime(),
            EndAt = new DateTime().AddHours(1).ToUniversalTime(),
            TotalSeats = 10
        };
        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Art Exhibition",
            Description = "An exhibition of modern art.",
            StartAt = new DateTime().ToUniversalTime(),
            EndAt = new DateTime().AddHours(2).ToUniversalTime(),
            TotalSeats = 20
        };

        await arrangeContext.Events.AddRangeAsync(event1, event2);
        arrangeContext.SaveChanges();

        var getQuery = new GetQuery
        {
            Title = "Concert"
        };

        // Act
        PaginatedResultDto result = await eventService.GetAllEvents(getQuery);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Events);
        Assert.Equal(event1.Title, result.Events.FirstOrDefault()!.Title);
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task GetAllEvents_WithDateRangeQuery_ReturnsEventsWithinDateRange()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var eventRepository = new EventRepository(actContext);
        var mockLogger = new Mock<ILogger<EventService>>();
        var eventService = new EventService(eventRepository, mockLogger.Object);
        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Morning Yoga",
            Description = "A relaxing morning yoga session.",
            StartAt = new DateTime(2024, 6, 1, 8, 0, 0).ToUniversalTime(),
            EndAt = new DateTime(2024, 6, 1, 9, 0, 0).ToUniversalTime(),
            TotalSeats = 15
        };
        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Evening Meditation",
            Description = "A calming evening meditation session.",
            StartAt = new DateTime(2024, 6, 1, 18, 0, 0).ToUniversalTime(),
            EndAt = new DateTime(2024, 6, 1, 19, 0, 0).ToUniversalTime(),
            TotalSeats = 15
        };
        await arrangeContext.Events.AddRangeAsync(event1, event2);
        arrangeContext.SaveChanges();
        var getQuery1 = new GetQuery
        {
            From = new DateTime(2024, 6, 1, 7, 0, 0),
            To = new DateTime(2024, 6, 1, 12, 0, 0)
        };
        var getQuery2 = new GetQuery
        {
            From = new DateTime(2024, 6, 1, 17, 0, 0),
            To = new DateTime(2024, 6, 1, 20, 0, 0)
        };
        var getQuery3 = new GetQuery
        {
            From = new DateTime(2024, 6, 1, 12, 0, 0),
            To = new DateTime(2024, 6, 1, 17, 0, 0)
        };
        var getQuery4 = new GetQuery
        {
            From = new DateTime(2024, 6, 1, 7, 0, 0),
            To = new DateTime(2024, 6, 1, 20, 0, 0)
        };
        // Act
        var result1 = await eventService.GetAllEvents(getQuery1);
        var result2 = await eventService.GetAllEvents(getQuery2);
        var result3 = await eventService.GetAllEvents(getQuery3);
        var result4 = await eventService.GetAllEvents(getQuery4);

        // Assert
        Assert.NotNull(result1);
        Assert.Single(result1.Events);
        Assert.Equal(event1.Title, result1.Events.FirstOrDefault()!.Title);

        Assert.NotNull(result2);
        Assert.Single(result2.Events);
        Assert.Equal(event2.Title, result2.Events.FirstOrDefault()!.Title);

        Assert.NotNull(result3);
        Assert.Empty(result3.Events);

        Assert.NotNull(result4);
        Assert.Equal(2, result4.Events.Count());
    }

    [Fact]
    [Trait("Category", "EventService")]
    public async Task GetAllEvents_WithCombinedQuery_ReturnsCorrectEvents()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var eventRepository = new EventRepository(actContext);
        var mockLogger = new Mock<ILogger<EventService>>();
        var eventService = new EventService(eventRepository, mockLogger.Object);
        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Morning Yoga",
            Description = "A relaxing morning yoga session.",
            StartAt = new DateTime(2024, 6, 1, 8, 0, 0).ToUniversalTime(),
            EndAt = new DateTime(2024, 6, 1, 9, 0, 0).ToUniversalTime(),
            TotalSeats = 15
        };
        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Breakfast",
            Description = "A refreshing morning breakfast session.",
            StartAt = new DateTime(2024, 6, 1, 9, 0, 0).ToUniversalTime(),
            EndAt = new DateTime(2024, 6, 1, 10, 0, 0).ToUniversalTime(),
            TotalSeats = 15
        };
        var event3 = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Evening Meditation",
            Description = "A calming evening meditation session.",
            StartAt = new DateTime(2024, 6, 1, 18, 0, 0).ToUniversalTime(),
            EndAt = new DateTime(2024, 6, 1, 19, 0, 0).ToUniversalTime(),
            TotalSeats = 15
        };
        await arrangeContext.Events.AddRangeAsync(event1, event2, event3);
        arrangeContext.SaveChanges();
        var getQuery = new GetQuery
        {
            Title = "br",
            To = new DateTime(2024, 6, 1, 12, 0, 0)
        };
        // Act
        var result = await eventService.GetAllEvents(getQuery);
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Events);
        Assert.Equal(event2.Title, result.Events.FirstOrDefault()!.Title);
    }
}

