namespace BookIt.Models.DTOs;

public class BusinessDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
