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
        [FromQuery] string? search = null)
    {
        var response = await businessService.GetBusinessesAsync(pageNumber, pageSize, category, search);
        return Ok(response);
    }
}
