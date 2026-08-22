using EventMicroService.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventMicroService.Application;

public interface IEventBookingRepository
{
    public Task<EventBooking?> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default);

    public Task<EventBooking?> GetEventBookingsAsync(Guid eventId, Guid bookingId, CancellationToken ct = default);

    public Task DeleteAsync(EventBooking eventBooking, CancellationToken ct = default);

    public Task CreateAsync(EventBooking eventBooking, CancellationToken ct = default);
}
