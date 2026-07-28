using BookIt.Models.Enums;

namespace BookIt.Models;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public Guid ClientId { get; set; }
    public User Client { get; set; } = null!;
    public DateTimeOffset StartTime { get; set; }

    // Fixed at booking time from Service.DurationMinutes, not recomputed later,
    // so later changes to Service.DurationMinutes don't affect existing bookings.
    public DateTimeOffset EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
