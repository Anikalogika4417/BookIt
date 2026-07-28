using BookIt.Extensions;
using BookIt.Models.Enums;
using BookIt.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.Controllers;

[ApiController]
[Authorize]
[Route("api/businesses")]
public class BusinessesController(IBusinessService businessService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBusinesses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var response = await businessService.GetBusinessesAsync(pageNumber, pageSize, category, search, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}/summary")]
    [Authorize(Roles = nameof(UserRole.BusinessOwner))]
    public async Task<IActionResult> GetBusinessSummary(Guid id, CancellationToken cancellationToken = default)
    {
        var (result, response) = await businessService.GetBusinessSummaryAsync(id, User.GetUserId(), cancellationToken);

        return result switch
        {
            BusinessSummaryResult.NotFound => NotFound("Business not found."),
            BusinessSummaryResult.Forbidden => Forbid(),
            _ => Ok(response)
        };
    }
}
