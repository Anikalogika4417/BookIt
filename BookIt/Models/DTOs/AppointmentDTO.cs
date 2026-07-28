using BookIt.Models.Enums;

namespace BookIt.Models.DTOs;

public class AppointmentDTO
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public decimal Price { get; set; }
    public AppointmentStatus Status { get; set; }
}
