using BookIt.Data;
using BookIt.Models;
using BookIt.Models.DTOs.Request;
using BookIt.Models.Enums;
using BookIt.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests;

public class AppointmentServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static AppointmentService CreateService(AppDbContext context) =>
        new(context, NullLogger<AppointmentService>.Instance);

    private static User SeedUser(AppDbContext context, UserRole role)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid()}@test.com",
            FullName = "Test User",
            Role = role
        };
        context.Users.Add(user);
        return user;
    }

    private static (Business Business, Service Service) SeedBusinessWithService(
        AppDbContext context, Guid ownerId, int durationMinutes = 60, decimal price = 100m)
    {
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = "Test Business",
            Category = "Beauty",
            OwnerId = ownerId
        };
        var service = new Service
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "Haircut",
            DurationMinutes = durationMinutes,
            Price = price
        };
        context.Businesses.Add(business);
        context.Services.Add(service);
        return (business, service);
    }

    private static Appointment AddAppointment(
        AppDbContext context, Guid serviceId, Guid clientId, DateTimeOffset start, DateTimeOffset end,
        AppointmentStatus status = AppointmentStatus.Confirmed)
    {
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            ServiceId = serviceId,
            ClientId = clientId,
            StartTime = start,
            EndTime = end,
            Status = status
        };
        context.Appointments.Add(appointment);
        return appointment;
    }

    [Fact]
    public async Task CreateAppointmentAsync_ValidRequest_ReturnsSuccessAndSavesAppointment()
    {
        await using var context = CreateContext();
        var owner = SeedUser(context, UserRole.BusinessOwner);
        var (_, service) = SeedBusinessWithService(context, owner.Id, durationMinutes: 30);
        var client = SeedUser(context, UserRole.Client);
        await context.SaveChangesAsync();

        var startTime = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
        var result = await CreateService(context).CreateAppointmentAsync(
            client.Id, new AppointmentRequest { ServiceId = service.Id, StartTime = startTime });

        Assert.Equal(CreateAppointmentResult.Success, result);
        var saved = Assert.Single(context.Appointments);
        Assert.Equal(startTime.AddMinutes(30), saved.EndTime);
    }

    [Fact]
    public async Task CreateAppointmentAsync_ServiceDoesNotExist_ReturnsServiceNotFound()
    {
        await using var context = CreateContext();
        var client = SeedUser(context, UserRole.Client);
        await context.SaveChangesAsync();

        var result = await CreateService(context).CreateAppointmentAsync(
            client.Id, new AppointmentRequest { ServiceId = Guid.NewGuid(), StartTime = DateTimeOffset.UtcNow });

        Assert.Equal(CreateAppointmentResult.ServiceNotFound, result);
    }

    [Fact]
    public async Task CreateAppointmentAsync_ServiceIsDeleted_ReturnsServiceNotFound()
    {
        await using var context = CreateContext();
        var owner = SeedUser(context, UserRole.BusinessOwner);
        var (_, service) = SeedBusinessWithService(context, owner.Id);
        service.IsDeleted = true;
        var client = SeedUser(context, UserRole.Client);
        await context.SaveChangesAsync();

        var result = await CreateService(context).CreateAppointmentAsync(
            client.Id, new AppointmentRequest { ServiceId = service.Id, StartTime = DateTimeOffset.UtcNow });

        Assert.Equal(CreateAppointmentResult.ServiceNotFound, result);
    }

    [Fact]
    public async Task CreateAppointmentAsync_OverlapsExistingAppointmentOnSameService_ReturnsServiceOverlap()
    {
        await using var context = CreateContext();
        var owner = SeedUser(context, UserRole.BusinessOwner);
        var (_, service) = SeedBusinessWithService(context, owner.Id, durationMinutes: 60);
        var existingClient = SeedUser(context, UserRole.Client);
        var newClient = SeedUser(context, UserRole.Client);

        var existingStart = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
        AddAppointment(context, service.Id, existingClient.Id, existingStart, existingStart.AddMinutes(60));
        await context.SaveChangesAsync();

        // Starts 30 minutes into the existing booking for the same service.
        var result = await CreateService(context).CreateAppointmentAsync(
            newClient.Id, new AppointmentRequest { ServiceId = service.Id, StartTime = existingStart.AddMinutes(30) });

        Assert.Equal(CreateAppointmentResult.ServiceOverlap, result);
    }

    [Fact]
    public async Task CreateAppointmentAsync_OverlapsExistingAppointmentForSameClient_ReturnsClientOverlap()
    {
        await using var context = CreateContext();
        var owner = SeedUser(context, UserRole.BusinessOwner);
        var (business, serviceA) = SeedBusinessWithService(context, owner.Id, durationMinutes: 60);
        var serviceB = new Service
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "Massage",
            DurationMinutes = 60,
            Price = 50m
        };
        context.Services.Add(serviceB);
        var client = SeedUser(context, UserRole.Client);

        var existingStart = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
        AddAppointment(context, serviceA.Id, client.Id, existingStart, existingStart.AddMinutes(60));
        await context.SaveChangesAsync();

        // Same client books a different service that overlaps in time.
        var result = await CreateService(context).CreateAppointmentAsync(
            client.Id, new AppointmentRequest { ServiceId = serviceB.Id, StartTime = existingStart.AddMinutes(30) });

        Assert.Equal(CreateAppointmentResult.ClientOverlap, result);
    }

    [Fact]
    public async Task CreateAppointmentAsync_BackToBackWithExistingAppointment_ReturnsSuccess()
    {
        await using var context = CreateContext();
        var owner = SeedUser(context, UserRole.BusinessOwner);
        var (_, service) = SeedBusinessWithService(context, owner.Id, durationMinutes: 60);
        var existingClient = SeedUser(context, UserRole.Client);
        var newClient = SeedUser(context, UserRole.Client);

        var existingStart = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
        var existingEnd = existingStart.AddMinutes(60);
        AddAppointment(context, service.Id, existingClient.Id, existingStart, existingEnd);
        await context.SaveChangesAsync();

        // Starts exactly when the previous appointment ends — must not count as an overlap.
        var result = await CreateService(context).CreateAppointmentAsync(
            newClient.Id, new AppointmentRequest { ServiceId = service.Id, StartTime = existingEnd });

        Assert.Equal(CreateAppointmentResult.Success, result);
    }

    [Fact]
    public async Task CreateAppointmentAsync_OverlapsOnlyWithCancelledAppointment_ReturnsSuccess()
    {
        await using var context = CreateContext();
        var owner = SeedUser(context, UserRole.BusinessOwner);
        var (_, service) = SeedBusinessWithService(context, owner.Id, durationMinutes: 60);
        var existingClient = SeedUser(context, UserRole.Client);
        var newClient = SeedUser(context, UserRole.Client);

        var existingStart = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
        AddAppointment(
            context, service.Id, existingClient.Id, existingStart, existingStart.AddMinutes(60),
            AppointmentStatus.Cancelled);
        await context.SaveChangesAsync();

        var result = await CreateService(context).CreateAppointmentAsync(
            newClient.Id, new AppointmentRequest { ServiceId = service.Id, StartTime = existingStart.AddMinutes(30) });

        Assert.Equal(CreateAppointmentResult.Success, result);
    }

    [Fact]
    public async Task GetAppointmentsAsync_AsClient_ReturnsOnlyOwnAppointments()
    {
        await using var context = CreateContext();
        var owner = SeedUser(context, UserRole.BusinessOwner);
        var (_, service) = SeedBusinessWithService(context, owner.Id);
        var client = SeedUser(context, UserRole.Client);
        var otherClient = SeedUser(context, UserRole.Client);

        var now = DateTimeOffset.UtcNow;
        AddAppointment(context, service.Id, client.Id, now, now.AddMinutes(60));
        AddAppointment(context, service.Id, otherClient.Id, now, now.AddMinutes(60));
        await context.SaveChangesAsync();

        var response = await CreateService(context).GetAppointmentsAsync(
            client.Id, UserRole.Client, from: null, to: null, status: null, pageNumber: 1, pageSize: 10);

        Assert.Equal(1, response.TotalCount);
        Assert.Single(response.Appointments);
    }

    [Fact]
    public async Task GetAppointmentsAsync_AsBusinessOwner_ReturnsAppointmentsAcrossAllOwnedBusinesses()
    {
        await using var context = CreateContext();
        var owner = SeedUser(context, UserRole.BusinessOwner);
        var (_, serviceA) = SeedBusinessWithService(context, owner.Id);

        // Second business owned by the same owner (1:many owner-to-business).
        var businessB = new Business { Id = Guid.NewGuid(), Name = "Second Business", Category = "Fitness", OwnerId = owner.Id };
        var serviceB = new Service { Id = Guid.NewGuid(), BusinessId = businessB.Id, Name = "Training", DurationMinutes = 45, Price = 80m };
        context.Businesses.Add(businessB);
        context.Services.Add(serviceB);

        // Unrelated business owned by someone else.
        var otherOwner = SeedUser(context, UserRole.BusinessOwner);
        var (_, unrelatedService) = SeedBusinessWithService(context, otherOwner.Id);

        var client = SeedUser(context, UserRole.Client);

        var now = DateTimeOffset.UtcNow;
        AddAppointment(context, serviceA.Id, client.Id, now, now.AddMinutes(60));
        AddAppointment(context, serviceB.Id, client.Id, now, now.AddMinutes(45));
        AddAppointment(context, unrelatedService.Id, client.Id, now, now.AddMinutes(60));
        await context.SaveChangesAsync();

        var response = await CreateService(context).GetAppointmentsAsync(
            owner.Id, UserRole.BusinessOwner, from: null, to: null, status: null, pageNumber: 1, pageSize: 10);

        Assert.Equal(2, response.TotalCount);
    }
}
