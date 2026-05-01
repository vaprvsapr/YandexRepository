using System.ComponentModel.DataAnnotations;

namespace EventManager.Models.Bookings
{
    public class BookingDto
    {
        [Required(ErrorMessage = "EventId обязателен для заполнения.")]
        public Guid EventId { get; init; }
        public Guid Id { get; init; } = Guid.Empty;
        public BookingStatus Status { get; init; } = BookingStatus.Pending;
        public DateTime CreatedAt { get; init; }
        public DateTime ProcessedAt { get; set; }
    }
}
