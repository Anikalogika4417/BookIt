using BookIt.Data;
using BookIt.Models.DTOs;
using BookIt.Models.DTOs.Response;
using BookIt.Models.Enums;
using BookIt.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.Services;

public class BusinessService(AppDbContext context) : IBusinessService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public async Task<BusinessResponse> GetBusinessesAsync(
        int pageNumber, int pageSize, string? category, string? search, CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var query = context.Businesses.Where(b => !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(b => EF.Functions.ILike(b.Name, $"%{search}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var businesses = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BusinessDTO
            {
                Id = b.Id,
                Name = b.Name,
                Category = b.Category
            })
            .ToListAsync(cancellationToken);

        return new BusinessResponse
        {
            Businesses = businesses,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<(BusinessSummaryResult Result, GetSummaryResponse? Response)> GetBusinessSummaryAsync(
        Guid businessId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var business = await context.Businesses
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == businessId && !b.IsDeleted, cancellationToken);

        if (business is null)
        {
            return (BusinessSummaryResult.NotFound, null);
        }

        if (business.OwnerId != requestingUserId)
        {
            return (BusinessSummaryResult.Forbidden, null);
        }

        var now = DateTimeOffset.UtcNow;

        var services = await context.Services
            .Where(s => s.BusinessId == businessId && !s.IsDeleted)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Price,
                UpcomingCount = s.Appointments.Count(a => a.StartTime > now && a.Status != AppointmentStatus.Cancelled)
            })
            .Select(x => new ServiceSummaryDTO
            {
                ServiceId = x.Id,
                ServiceName = x.Name,
                UpcomingAppointmentsCount = x.UpcomingCount,
                ExpectedRevenue = x.UpcomingCount * x.Price
            })
            .ToListAsync(cancellationToken);

        return (BusinessSummaryResult.Success, new GetSummaryResponse { Services = services });
    }
}
