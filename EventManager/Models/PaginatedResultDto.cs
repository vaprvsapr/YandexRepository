namespace EventManager.Models;
using EventManager.Models;

public class PaginatedResultDto
{
    public IEnumerable<EventDto> Events { get; init; }

    public int TotalCount { get; init; }

    public int PageSize { get; init; } = 10;

    public int Page { get; init; } = 1;

}
