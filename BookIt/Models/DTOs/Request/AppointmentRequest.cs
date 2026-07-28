namespace BookIt.Models.DTOs.Request;

public class AppointmentRequest
{
    public Guid ServiceId { get; set; }
    public DateTimeOffset StartTime { get; set; }
}
