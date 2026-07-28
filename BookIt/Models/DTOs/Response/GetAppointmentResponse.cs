namespace BookIt.Models.DTOs.Response;

public class GetAppointmentResponse
{
    public List<AppointmentDTO> Appointments { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
