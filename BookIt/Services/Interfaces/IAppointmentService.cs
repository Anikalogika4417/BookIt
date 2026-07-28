using BookIt.Models.DTOs.Request;
using BookIt.Models.DTOs.Response;
using BookIt.Models.Enums;

namespace BookIt.Services.Interfaces;

public interface IAppointmentService
{
    Task<CreateAppointmentResult> CreateAppointmentAsync(
        Guid clientId, AppointmentRequest request, CancellationToken cancellationToken = default);

    Task<GetAppointmentResponse> GetAppointmentsAsync(
        Guid userId,
        UserRole role,
        DateTimeOffset? from,
        DateTimeOffset? to,
        AppointmentStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
