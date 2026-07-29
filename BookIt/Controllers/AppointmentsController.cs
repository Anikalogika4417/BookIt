using BookIt.Extensions;
using BookIt.Models.DTOs.Request;
using BookIt.Models.Enums;
using BookIt.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.Controllers;

[ApiController]
[Authorize]
[Route("api/appointments")]
public class AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> CreateAppointment(AppointmentRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received request {Action}", nameof(CreateAppointment));

        var clientId = User.GetUserId();

        var result = await appointmentService.CreateAppointmentAsync(clientId, request, cancellationToken);

        return result switch
        {
            CreateAppointmentResult.ServiceNotFound => NotFound("Service not found."),
            CreateAppointmentResult.ServiceOverlap => Conflict("This service is already booked during this time."),
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
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received request {Action}", nameof(GetAppointments));

        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return BadRequest("'from' must not be later than 'to'. Please swap the values.");
        }

        var userId = User.GetUserId();
        var role = User.GetRole();

        var response = await appointmentService.GetAppointmentsAsync(
            userId, role, from, to, status, pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }
}
