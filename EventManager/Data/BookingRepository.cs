using EventManager.Interfaces;
using EventManager.Models.Bookings;
using System.Collections.Concurrent;

namespace EventManager.Data;

public class BookingRepository : IRepository<Booking>
{
    private ConcurrentDictionary<Guid, Booking> _bookingsDictionary = [];
    public void Add(Booking entityToAdd)
    {
        _bookingsDictionary[entityToAdd.Id] = entityToAdd;
    }

    public void Delete(Booking entityToDelete)
    {
        _bookingsDictionary.TryRemove(entityToDelete.Id, out _);
    }

    public IReadOnlyCollection<Booking> GetAll()
    {
        return _bookingsDictionary.Values.ToList().AsReadOnly();
    }

    public Booking? GetById(Guid id)
    {
        return _bookingsDictionary.GetValueOrDefault(id);
    }

    public void Update(Guid id, Booking entityToUpdate)
    {
        var booking = GetById(id);
        if (booking != null)
        {
            _bookingsDictionary[booking.Id] = entityToUpdate;
        }
    }
}
