using System.Security.Claims;
using BookIt.Models.DTOs.Request;
using BookIt.Models.Enums;
using BookIt.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.Controllers;

[ApiController]
[Authorize]
[Route("api/appointments")]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> CreateAppointment(AppointmentRequest request)
    {
        var clientId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await appointmentService.CreateAppointmentAsync(clientId, request);

        return result switch
        {
            CreateAppointmentResult.ServiceNotFound => NotFound("Service not found."),
            CreateAppointmentResult.OwnerOverlap => Conflict("The business owner already has an appointment during this time."),
            CreateAppointmentResult.ClientOverlap => Conflict("You already have an appointment during this time."),
            _ => Ok()
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] AppointmentStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

        var response = await appointmentService.GetAppointmentsAsync(userId, role, from, to, status, pageNumber, pageSize);
        return Ok(response);
    }
}
