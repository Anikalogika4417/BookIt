using BookIt.Models.DTOs.Response;

namespace BookIt.Services.Interfaces;

public interface IBusinessService
{
    Task<BusinessResponse> GetBusinessesAsync(int pageNumber, int pageSize, string? category, string? search);
}
