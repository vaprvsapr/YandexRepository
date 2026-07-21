using EventManager.Domain.Models;
using EventManager.Application.Dto;
using EventManager.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Tests.Integration;

[Collection("Postgres collection")]
public class EventRepositoryTests(PostgresFixture postgresFixture) : PostgresTest(postgresFixture)
{
    [Fact]
    [Trait("Category", "EventRepository")]
    public async Task CreateEvent_SavesEventToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();

        var newEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            TotalSeats = 100
        };

        var eventRepository = new EventRepository(context);

        // Act
        await eventRepository.CreateAsync(newEvent);

        // Assert
        await using var verifyingContext = await CreateContextAsync();
        var createdEvent = await verifyingContext.Events.FirstOrDefaultAsync(e => e.Id == newEvent.Id);
        Assert.NotNull(createdEvent);
        Assert.Equal(newEvent.Title, createdEvent.Title);
        Assert.Equal(newEvent.Description, createdEvent.Description);
        Assert.InRange((TimeSpan)(newEvent.StartAt - createdEvent.StartAt)!, TimeSpan.Zero, TimeSpan.FromMicroseconds(1));
        Assert.InRange((TimeSpan)(newEvent.EndAt - createdEvent.EndAt)!, TimeSpan.Zero, TimeSpan.FromMicroseconds(1));
    }

    [Fact]
    [Trait("Category", "EventRepository")]
    public async Task CreateEvent_CreatingDuplicateEvent_ThrowsInvalidOperationException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();

        var newEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            TotalSeats = 100
        };

        var eventRepository = new EventRepository(context);
        await eventRepository.CreateAsync(newEvent);
        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await eventRepository.CreateAsync(newEvent));
    }

    [Fact]
    [Trait("Category", "EventRepository")]
    public async Task GetByIdAsync_ReturnsEvent_WhenEventExists()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var existingEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Existing Event",
            Description = "Existing event description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            TotalSeats = 100
        };
        context.Events.Add(existingEvent);
        await context.SaveChangesAsync();
        await using var retrievingContext = await CreateContextAsync();
        var eventRepository = new EventRepository(retrievingContext);
        // Act
        var retrievedEvent = await eventRepository.GetByIdAsync(existingEvent.Id);
        // Assert
        Assert.NotNull(retrievedEvent);
        Assert.Equal(existingEvent.Id, retrievedEvent.Id);
        Assert.Equal(existingEvent.Title, retrievedEvent.Title);
        Assert.Equal(existingEvent.Description, retrievedEvent.Description);
    }

    [Fact]
    [Trait("Category", "EventRepository")]
    public async Task GetByIdAsync_WithInvalidEventId_ThrowsKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var eventRepository = new EventRepository(context);
        var nonExistentEventId = Guid.NewGuid();
        // Act & Assert
        var nonExistenEvent = await eventRepository.GetByIdAsync(nonExistentEventId);
        Assert.Null(nonExistenEvent);
    }

    [Fact]
    [Trait("Category", "EventRepository")]
    public async Task DeleteEvent_RemovesEventFromDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var eventToDelete = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Event to Delete",
            Description = "Event to delete description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            TotalSeats = 100
        };
        context.Events.Add(eventToDelete);
        await context.SaveChangesAsync();
        await using var deletingContext = await CreateContextAsync();
        var eventRepository = new EventRepository(deletingContext);
        // Act
        await eventRepository.DeleteAsync(eventToDelete);
        // Assert
        await using var verifyingContext = await CreateContextAsync();
        var deletedEvent = await verifyingContext.Events.FirstOrDefaultAsync(e => e.Id == eventToDelete.Id);
        Assert.Null(deletedEvent);
    }

    [Fact]
    [Trait("Category", "EventRepository")]
    public async Task DeleteEvent_DeletingNonExistentEvent_ThrowsKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var eventRepository = new EventRepository(context);
        var nonExistententEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "title",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            TotalSeats = 100
        };
        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            await eventRepository.DeleteAsync(nonExistententEvent));
    }

    [Fact]
    [Trait("Category", "EventRepository")]
    public async Task GetAll_ReturnsAllEvents()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var events = new List<Event>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Event 1",
                Description = "Description 1",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddHours(1),
                TotalSeats = 100
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Event 2",
                Description = "Description 2",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddHours(2),
                TotalSeats = 150
            }
        };
        context.Events.AddRange(events);
        await context.SaveChangesAsync();
        await using var retrievingContext = await CreateContextAsync();
        var eventRepository = new EventRepository(retrievingContext);
        // Act
        var retrievedEvents = await eventRepository.GetAll().ToListAsync();
        // Assert
        Assert.Equal(events.Count, retrievedEvents.Count);
        foreach (var evt in events)
        {
            Assert.Contains(retrievedEvents, e => e.Id == evt.Id && e.Title == evt.Title && e.Description == evt.Description);
        }
    }
}
