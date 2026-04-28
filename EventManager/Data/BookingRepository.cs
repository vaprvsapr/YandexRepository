using EventManager.Interfaces;
using EventManager.Models;

namespace EventManager.Data;

public class BookingRepository : IRepository<Booking>
{
    private List<Booking> _bookings = [];
    public void Add(Booking entityToAdd)
    {
        _bookings.Add(entityToAdd);
    }

    public void Delete(Booking entityToDelete)
    {
        _bookings.Remove(entityToDelete);
    }

    public IReadOnlyCollection<Booking> GetAll()
    {
        return _bookings.AsReadOnly();
    }

    public Booking? GetById(int id)
    {
        return _bookings.FirstOrDefault(b => b.Id == id);
    }

    public void Update(int id, Booking entityToUpdate)
    {
        var booking = GetById(id);
        if (booking != null)
        {
            _bookings.Remove(booking);
            _bookings.Add(entityToUpdate);
        }
    }
}
