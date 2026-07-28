namespace BookIt.Models;

public class Service
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
