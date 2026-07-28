using BookIt.Models.DTOs.Response;
using BookIt.Models.Enums;

namespace BookIt.Services.Interfaces;

public interface IBusinessService
{
    Task<BusinessResponse> GetBusinessesAsync(int pageNumber, int pageSize, string? category, string? search);

    Task<(BusinessSummaryResult Result, GetSummaryResponse? Response)> GetBusinessSummaryAsync(
        Guid businessId, Guid requestingUserId);
}
