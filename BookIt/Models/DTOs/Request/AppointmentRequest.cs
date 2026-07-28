namespace BookIt.Models.DTOs.Request;

public class AppointmentRequest
{
    public int ServiceId { get; set; }
    public DateTimeOffset StartTime { get; set; }
}
