using EventMicroService.Application;
using EventMicroService.Domain;
using StackExchange.Redis;
using System.Text.Json;

namespace EventMicroService.Infrastructure;

public class CacheRepository(IConnectionMultiplexer connectionMultiplexer) : ICacheRepository
{
    private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();
    private static readonly TimeSpan _ttl = TimeSpan.FromSeconds(10);

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        var key = $"event:{id}";

        var cached = await _redis.StringGetAsync(key);
        if (cached.HasValue)
            return JsonSerializer.Deserialize<Event>((string)cached!);
        return null;
    }

    public async Task SaveEventAsync(Event @event)
    {
        var key = $"event:{@event.Id}";
        var serialized = JsonSerializer.Serialize(@event);
        await _redis.StringSetAsync(key, serialized, _ttl);
    }

    public async Task<List<Event>?> GetTop10Async()
    {
        var key = "events:top10";

        var cached = await _redis.StringGetAsync(key);
        if (cached.HasValue)
            return JsonSerializer.Deserialize<List<Event>>((string)cached!);

        return null;
    }

    public async Task SaveTop10Async(List<Event> top10Events)
    {
        var key = "events:top10";

        var serialized = JsonSerializer.Serialize(top10Events);
        await _redis.StringSetAsync(key, serialized, _ttl);
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        var key = $"event:{id}";

        await _redis.KeyDeleteAsync(key);
    }
}
