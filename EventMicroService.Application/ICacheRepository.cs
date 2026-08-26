using EventMicroService.Domain;
namespace EventMicroService.Application;

public interface ICacheRepository
{
    Task<Event?> GetByIdAsync(Guid id);

    Task SaveEventAsync(Event @event);
    Task<List<Event>?> GetTop10Async();
    Task SaveTop10Async(List<Event>);

    Task DeleteByIdAsync(Guid id);
}
