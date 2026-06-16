using EventManager.DataAccess;
using EventManager.DataAccess.Repositories;
using EventManager.Models.Events;
using EventManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;

namespace EventManager.IntegrationTests;

public class EventRepositoryTests : PostgresTest
{
    public EventRepositoryTests(PostgresFixture postgresFixture) : base(postgresFixture)
    {
    }

    [Fact]
    [Trait("Category", "EventRepository")]
    public async Task CreateEvent_SavesEventToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();

        var eventCreateDto = new EventCreateDto
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
        };

        var eventRepository = new EventRepository(context);

        // Act
        await eventRepository.CreateAsync(eventCreateDto);

        // Assert
        await using var verifyingContext = await CreateContextAsync();
        var createdEvent = await verifyingContext.Events.FirstOrDefaultAsync(e => e.Id == eventCreateDto.Id);
        Assert.NotNull(createdEvent);
        Assert.Equal(eventCreateDto.Title, createdEvent.Title);
        Assert.Equal(eventCreateDto.Description, createdEvent.Description);
        Assert.InRange((TimeSpan)(eventCreateDto.StartAt - createdEvent.StartAt)!, TimeSpan.Zero, TimeSpan.FromMicroseconds(1));
        Assert.InRange((TimeSpan)(eventCreateDto.EndAt - createdEvent.EndAt)!, TimeSpan.Zero, TimeSpan.FromMicroseconds(1));
    }

    [Fact]
    [Trait("Category", "EventRepository")]
    public async Task CreateEvent_CreatingDuplicateEvent_ThrowsInvalidOperationException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var eventCreateDto = new EventCreateDto
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
        };
        var eventRepository = new EventRepository(context);
        await eventRepository.CreateAsync(eventCreateDto);
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await eventRepository.CreateAsync(eventCreateDto));
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
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await eventRepository.GetByIdAsync(nonExistentEventId));
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
        await eventRepository.DeleteByIdAsync(eventToDelete.Id);
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
        var nonExistentEventId = Guid.NewGuid();
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await eventRepository.DeleteByIdAsync(nonExistentEventId));
    }

    [Fact]
    [Trait("Category", "EventRepository")]
    public async Task UpdateEvent_UpdatesEventInDatabase()
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
        await using var updatingContext = await CreateContextAsync();
        var eventRepository = new EventRepository(updatingContext);
        var updatedEvent = new Event
        {
            Id = existingEvent.Id,
            Title = "Updated Event",
            Description = "Updated event description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow,
            TotalSeats = 150
        };
        // Act
        await eventRepository.UpdateAsync(updatedEvent);
        // Assert
        await using var verifyingContext = await CreateContextAsync();
        var retrievedEvent = await verifyingContext.Events.FirstOrDefaultAsync(e => e.Id == existingEvent.Id);
        Assert.NotNull(retrievedEvent);
        Assert.Equal(updatedEvent.Title, retrievedEvent.Title);
        Assert.Equal(updatedEvent.Description, retrievedEvent.Description);
        Assert.Equal(updatedEvent.TotalSeats, retrievedEvent.TotalSeats);
        Assert.InRange((TimeSpan)(updatedEvent.StartAt - retrievedEvent.StartAt)!, TimeSpan.Zero, TimeSpan.FromMicroseconds(1));
        Assert.InRange((TimeSpan)(updatedEvent.EndAt - retrievedEvent.EndAt)!, TimeSpan.Zero, TimeSpan.FromMicroseconds(1));
    }

    [Fact]
    [Trait("Category", "EventRepository")]
    public async Task UpdateEvent_UpdatingNonExistentEvent_ThrowsKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var eventRepository = new EventRepository(context);
        var nonExistentEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Non-existent Event",
            Description = "Non-existent event description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            TotalSeats = 100
        };
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await eventRepository.UpdateAsync(nonExistentEvent));
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
