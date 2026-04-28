using System.ComponentModel.DataAnnotations;

namespace EventManager.Models.Bookings
{
    public class BookingDto
    {
        [Required(ErrorMessage = "EventId обязателен для заполнения.")]
        public int EventId { get; init; }
    }
}
