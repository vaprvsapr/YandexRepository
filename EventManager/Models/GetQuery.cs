namespace EventManager.Models;

using System.ComponentModel.DataAnnotations;

public class GetQuery
{
    public string? Title { get; init; } = null;

    public DateTime? From { get; init; } = null;

    public DateTime? To { get; init; } = null;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
