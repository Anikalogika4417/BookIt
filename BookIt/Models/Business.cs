namespace BookIt.Models;

public class Business
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<Service> Services { get; set; } = new List<Service>();
}
