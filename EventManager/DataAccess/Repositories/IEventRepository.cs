using EventManager.Models.Events;
namespace EventManager.DataAccess.Repositories;

public interface IEventRepository
{
    public Task<Event> GetByIdAsync(Guid id);

    public IQueryable<Event> GetAll();

    public Task DeleteByIdAsync(Guid id);

    public Task<Event> UpdateAsync(Event @event);

    public Task<Event> CreateAsync(EventCreateDto eventCreateDto);
}
