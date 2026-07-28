namespace BookIt.Models.DTOs.Response;

public class GetSummaryResponse
{
    public List<ServiceSummaryDTO> Services { get; set; } = new();
}
