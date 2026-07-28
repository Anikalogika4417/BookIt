using BookIt.Models.DTOs.Request;
using BookIt.Models.DTOs.Response;
using BookIt.Models.Enums;

namespace BookIt.Services.Interfaces;

public interface IAppointmentService
{
    Task<CreateAppointmentResult> CreateAppointmentAsync(Guid clientId, AppointmentRequest request);

    Task<GetAppointmentResponse> GetAppointmentsAsync(
        Guid userId,
        UserRole role,
        DateTimeOffset? from,
        DateTimeOffset? to,
        AppointmentStatus? status,
        int pageNumber,
        int pageSize);
}
