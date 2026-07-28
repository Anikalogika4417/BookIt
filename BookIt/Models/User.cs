using BookIt.Models.Enums;

namespace BookIt.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    // 1:1 — only populated when Role == BusinessOwner
    public Business? OwnedBusiness { get; set; }

    // Appointments booked by this user as a client
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
