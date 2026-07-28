using BookIt.Data;
using BookIt.Models.DTOs;
using BookIt.Models.DTOs.Response;
using BookIt.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.Services;

public class BusinessService(AppDbContext context) : IBusinessService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public async Task<BusinessResponse> GetBusinessesAsync(int pageNumber, int pageSize, string? category, string? search)
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

        var totalCount = await query.CountAsync();

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
            .ToListAsync();

        return new BusinessResponse
        {
            Businesses = businesses,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
