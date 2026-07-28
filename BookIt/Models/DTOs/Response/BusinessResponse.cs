namespace BookIt.Models.DTOs.Response;

public class BusinessResponse
{
    public List<BusinessDTO> Businesses { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
