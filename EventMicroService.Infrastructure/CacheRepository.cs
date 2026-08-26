using EventMicroService.Application;
using EventMicroService.Domain;
using StackExchange.Redis;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace EventMicroService.Infrastructure;

public class CacheRepository : ICacheRepository
{
    private readonly IEventRepository _eventRepository;
    private readonly IDatabase _redis;
    private static readonly TimeSpan _ttl = TimeSpan.FromSeconds(10);


    public CacheRepository(IConnectionMultiplexer connectionMultiplexer, IEventRepository eventRepository)
    {
        _redis = connectionMultiplexer.GetDatabase();
        _eventRepository = eventRepository;
    }
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
