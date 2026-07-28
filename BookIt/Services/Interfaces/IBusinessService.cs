using BookIt.Models.DTOs.Response;
using BookIt.Models.Enums;

namespace BookIt.Services.Interfaces;

public interface IBusinessService
{
    Task<BusinessResponse> GetBusinessesAsync(
        int pageNumber, int pageSize, string? category, string? search, CancellationToken cancellationToken = default);

    Task<(BusinessSummaryResult Result, GetSummaryResponse? Response)> GetBusinessSummaryAsync(
        Guid businessId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
