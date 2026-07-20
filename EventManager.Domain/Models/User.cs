namespace EventManager.Domain.Models;

public class User
{
    public required Guid Id { get; set; }
    public required string Login { get; set; }
    public required string PasswordHash { get; set; }
    public required UserRole Role { get; set; } 
    public List<Booking> Bookings { get; set; } = [];
}
