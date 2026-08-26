using EventMicroService.Domain;
namespace EventMicroService.Application;

public interface ICacheRepository
{
    Task<Event?> GetByIdAsync(Guid id);

    Task SaveEventAsync(Event @event);
    Task<List<Event>?> GetTop10Async();
    Task SaveTop10Async(List<Event> top10Events);

    Task DeleteByIdAsync(Guid id);
}
