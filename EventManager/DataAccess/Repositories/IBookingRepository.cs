using EventManager.Models.Bookings;
namespace EventManager.DataAccess.Repositories;

public interface IBookingRepository
{
    public Task<Booking> GetByIdAsync(Guid id);

    public Task<IEnumerable<Booking>> GetBookingsByEventIdAsync(Guid eventId);
    public IQueryable<Booking> GetAll();
    public Task DeleteByIdAsync(Guid id);
    public Task<Booking> CreateAsync(Guid eventId);
}
