using EventManager.DataAccess.Repositories;
using EventManager.Services;
using EventManager.Models.Events;
using EventManager.Models.Queries;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventManager.IntegrationTests;

[Collection("Postgres collection")]
public class PaginationTests(PostgresFixture postgresFixture) : PostgresTest(postgresFixture)
{
    private static readonly int numberOfEvents = 25;
    public static IEnumerable<EventCreateDto> TestData()
    {
        for (int i = 0; i < numberOfEvents; i++)
            yield return new EventCreateDto
            {
                Id = Guid.NewGuid(),
                Title = $"Test Event {i}",
                Description = "Test Description",
                StartAt = DateTime.Now.ToUniversalTime(),
                EndAt = DateTime.Now.AddHours(1).ToUniversalTime(),
                TotalSeats = i
            };
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [Trait("Category", "Pagination")]
    public async Task Pagination_VaryingPageNumber_ShouldReturnCorrectNumberOfItemsOnEachPage(int pageNumber)
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();

        var repository = new EventRepository(context);
        var mockLogger = new Mock<ILogger<EventService>>();
        var eventsService = new EventService(repository, mockLogger.Object);
        var query = new GetQuery() { Page = pageNumber };

        foreach (var eventCreateDto in TestData())
            await eventsService.CreateEvent(eventCreateDto);

        // Act
        var page = await eventsService.GetAllEvents(query);
        // Assert
        var expectedCount = pageNumber * query.PageSize > numberOfEvents
            ? int.Max(0, numberOfEvents - (pageNumber-1) * query.PageSize) 
            : query.PageSize;
        Assert.Equal(expectedCount, page.Events.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    [InlineData(25)]
    [InlineData(30)]
    [Trait("Category", "Pagination")]
    public async Task Pagination_VaryingPageSize_ShouldReturnCorrectNumberOfItemsOnEachPage(int pageSize)
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        var repository = new EventRepository(context);
        var mockLogger = new Mock<ILogger<EventService>>();
        var eventsService = new EventService(repository, mockLogger.Object);
        var numberOfPages = (int)Math.Ceiling(numberOfEvents / (double)pageSize);

        foreach (var eventCreateDto in TestData())
            await eventsService.CreateEvent(eventCreateDto);

        // Act & Assert
        for (int page = 1; page <= numberOfPages; page++)
        {
            var result = await eventsService.GetAllEvents(new GetQuery() { Page = page, PageSize = pageSize });
            var expectedCount = page < numberOfPages 
                ? pageSize 
                : numberOfEvents - (page - 1) * pageSize;
            Assert.Equal(expectedCount, result.Events.Count());
        }
    }
}
