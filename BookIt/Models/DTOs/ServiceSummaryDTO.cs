namespace BookIt.Models.DTOs;

public class ServiceSummaryDTO
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int UpcomingAppointmentsCount { get; set; }
    public decimal ExpectedRevenue { get; set; }
}
