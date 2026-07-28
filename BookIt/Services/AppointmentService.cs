using BookIt.Data;
using BookIt.Models;
using BookIt.Models.DTOs;
using BookIt.Models.DTOs.Request;
using BookIt.Models.DTOs.Response;
using BookIt.Models.Enums;
using BookIt.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookIt.Services;

public class AppointmentService(AppDbContext context) : IAppointmentService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;


    public async Task<CreateAppointmentResult> CreateAppointmentAsync(Guid clientId, AppointmentRequest request)
    {
        var service = await context.Services
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == request.ServiceId && !s.IsDeleted);

        if (service is null)
        {
            return CreateAppointmentResult.ServiceNotFound;
        }

        var endTime = request.StartTime.AddMinutes(service.DurationMinutes);

        // Overlap check across the whole business: the owner performs every service
        // of their business, so they cannot be double-booked regardless of which service it is.
        var ownerOverlap = await context.Appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .Where(a => a.Service.BusinessId == service.BusinessId)
            .AnyAsync(a => a.StartTime < endTime && request.StartTime < a.EndTime);

        if (ownerOverlap)
        {
            return CreateAppointmentResult.OwnerOverlap;
        }

        var clientOverlap = await context.Appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .Where(a => a.ClientId == clientId)
            .AnyAsync(a => a.StartTime < endTime && request.StartTime < a.EndTime);

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
        await context.SaveChangesAsync();

        return CreateAppointmentResult.Success;
    }

    public async Task<GetAppointmentResponse> GetAppointmentsAsync(
        Guid userId,
        UserRole role,
        DateTimeOffset? from,
        DateTimeOffset? to,
        AppointmentStatus? status,
        int pageNumber,
        int pageSize)
    {
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

        var totalCount = await query.CountAsync();

        var appointments = await query
            .OrderByDescending(a => a.StartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentDTO
            {
                Id = a.Id,
                ServiceName = a.Service.Name,
                BusinessName = a.Service.Business.Name,
                OwnerName = a.Service.Business.Owner.FullName,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Price = a.Service.Price,
                Status = a.Status
            })
            .ToListAsync();

        return new GetAppointmentResponse
        {
            Appointments = appointments,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
