using BookIt.Data;
using BookIt.Models;
using BookIt.Models.DTOs;
using BookIt.Models.DTOs.Request;
using BookIt.Models.DTOs.Response;
using BookIt.Models.Enums;
using BookIt.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.Services;

public class AppointmentService(AppDbContext context, ILogger<AppointmentService> logger) : IAppointmentService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public async Task<CreateAppointmentResult> CreateAppointmentAsync(
        Guid clientId, AppointmentRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Started {Method}", nameof(CreateAppointmentAsync));

        var service = await context.Services
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == request.ServiceId && !s.IsDeleted, cancellationToken);

        if (service is null)
        {
            return CreateAppointmentResult.ServiceNotFound;
        }

        var endTime = request.StartTime.AddMinutes(service.DurationMinutes);

        var serviceOverlap = await context.Appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .Where(a => a.ServiceId == service.Id)
            .AnyAsync(a => a.StartTime < endTime && request.StartTime < a.EndTime, cancellationToken);

        if (serviceOverlap)
        {
            return CreateAppointmentResult.ServiceOverlap;
        }

        var clientOverlap = await context.Appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .Where(a => a.ClientId == clientId)
            .AnyAsync(a => a.StartTime < endTime && request.StartTime < a.EndTime, cancellationToken);

        if (clientOverlap)
        {
            return CreateAppointmentResult.ClientOverlap;
        }

        var appointment = new Appointment
        {
            ServiceId = service.Id,
            ClientId = clientId,
            StartTime = request.StartTime,
            EndTime = endTime,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Finished {Method}", nameof(CreateAppointmentAsync));
        return CreateAppointmentResult.Success;
    }

    public async Task<GetAppointmentResponse> GetAppointmentsAsync(
        Guid userId,
        UserRole role,
        DateTimeOffset? from,
        DateTimeOffset? to,
        AppointmentStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Started {Method}", nameof(GetAppointmentsAsync));

        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var query = role == UserRole.Client
            ? context.Appointments.Where(a => a.ClientId == userId)
            : context.Appointments.Where(a => a.Service.Business.OwnerId == userId);

        if (from.HasValue)
        {
            query = query.Where(a => a.StartTime >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(a => a.EndTime <= to.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var appointments = await query
            .OrderBy(a => a.StartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentDTO
            {
                Id = a.Id,
                ServiceName = a.Service.Name,
                BusinessName = a.Service.Business.Name,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Price = a.Service.Price,
                Status = a.Status
            })
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "Finished {Method}, returned {Count} items", nameof(GetAppointmentsAsync), appointments.Count);

        return new GetAppointmentResponse
        {
            Appointments = appointments,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
