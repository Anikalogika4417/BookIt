using BookIt.Models.Enums;

namespace BookIt.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    // 1:many — populated when Role == BusinessOwner; an owner may run several businesses
    public ICollection<Business> OwnedBusinesses { get; set; } = new List<Business>();

    // Appointments booked by this user as a client
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
