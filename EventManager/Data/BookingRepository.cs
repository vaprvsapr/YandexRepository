using EventManager.Interfaces;
using EventManager.Models.Bookings;
using System.Collections.Concurrent;

namespace EventManager.Data;

/// <summary>
/// Репозиторий для управления данными бронирований, предоставляющий методы для получения, добавления, обновления и удаления бронирований.
/// </summary>
public class BookingRepository : IRepository<Booking>
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookingsDictionary = [];

    /// <summary>
    /// Добавляет новое бронирование в репозиторий.
    /// </summary>
    /// <param name="entityToAdd">Бронирование, которое требуется добавить.</param>
    public void Add(Booking entityToAdd)
    {
        _bookingsDictionary[entityToAdd.Id] = entityToAdd;
    }


    /// <summary>
    /// Удаляет бронирование из репозитория.
    /// </summary>
    /// <param name="entityToDelete">Бронирование, которое требуется удалить.</param>
    public void Delete(Booking entityToDelete)
    {
        _bookingsDictionary.TryRemove(entityToDelete.Id, out _);
    }


    /// <summary>
    /// Возвращает неизменяемую коллекцию всех бронирований.
    /// </summary>
    /// <returns>Коллекция объектов <see cref="Booking"/>. Если бронирования отсутствуют, возвращается пустая коллекция.</returns>
    public IReadOnlyCollection<Booking> GetAll()
    {
        return _bookingsDictionary.Values.ToList().AsReadOnly();
    }


    /// <summary>
    /// Возвращает бронирование с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор бронирования.</param>
    /// <returns>Объект <see cref="Booking"/>, если бронирование найдено; в противном случае — <see langword="null"/>.</returns>
    public Booking? GetById(Guid id)
    {
        return _bookingsDictionary.GetValueOrDefault(id);
    }


    /// <summary>
    /// Обновляет существующее бронирование с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор бронирования.</param>
    /// <param name="entityToUpdate">Новые данные бронирования.</param>
    public void Update(Guid id, Booking entityToUpdate)
    {
        var booking = GetById(id);
        if (booking != null)
        {
            _bookingsDictionary[booking.Id] = entityToUpdate;
        }
    }
}
