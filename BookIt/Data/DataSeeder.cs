using BookIt.Models;
using BookIt.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookIt.Data;

public static class DataSeeder
{
    private static readonly DateTimeOffset Aug7 = new(2026, 8, 7, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset RangeStart = new(2026, 6, 1, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset RangeEnd = new(2026, 9, 30, 18, 0, 0, TimeSpan.Zero);

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var users = CreateUsers();
        var (businesses, services) = CreateBusinessesAndServices(users);
        var appointments = CreateAppointments(businesses, services, users);

        context.Users.AddRange(users.Values);
        context.Businesses.AddRange(businesses);
        context.Services.AddRange(services);
        context.Appointments.AddRange(appointments);

        await context.SaveChangesAsync();
    }

    private static Dictionary<string, User> CreateUsers()
    {
        var now = DateTimeOffset.UtcNow;

        User NewUser(string fullName, string email, UserRole role, string password, bool isDeleted = false) => new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            Role = role,
            CreatedAt = now,
            IsDeleted = isDeleted
        };

        return new Dictionary<string, User>
        {
            // Business owners
            ["Noa"] = NewUser("Noa Peretz", "noa.peretz@bookitdemo.com", UserRole.BusinessOwner,
                "noa.peretz@bookitdemo.com_pass"),
            ["Itamar"] = NewUser("Itamar Katz", "itamar.katz@bookitdemo.com", UserRole.BusinessOwner,
                "itamar.katz@bookitdemo.com_pass"),
            ["Maya"] = NewUser("Maya Ben-David", "maya.bendavid@bookitdemo.com", UserRole.BusinessOwner,
                "maya.bendavid@bookitdemo.com_pass"),

            // Clients — Yael and Ori intentionally share the same password.
            ["Tal"] = NewUser("Tal Avidan", "tal.avidan@example.com", UserRole.Client,
                "tal.avidan@example.com_pass"),
            ["Adi"] = NewUser("Adi Nagar", "adi.nagar@example.com", UserRole.Client,
                "adi.nagar@example.com_pass"),
            ["Yael"] = NewUser("Yael Segal", "yael.segal@example.com", UserRole.Client,
                "ori.harel@example.com"),
            ["Ori"] = NewUser("Ori Harel", "ori.harel@example.com", UserRole.Client,
                "ori.harel@example.com"),
            ["Noga"] = NewUser("Noga Regev", "noga.regev@example.com", UserRole.Client,
                "noga.regev@example.com_pass", isDeleted: true)
        };
    }

    private static (List<Business> Businesses, List<Service> Services) CreateBusinessesAndServices(
        Dictionary<string, User> users)
    {
        var now = DateTimeOffset.UtcNow;
        var businesses = new List<Business>();
        var services = new List<Service>();
        var businessSequence = 0;

        Business NewBusiness(User owner, string name, string category)
        {
            businessSequence++;
            var business = new Business
            {
                Id = Guid.NewGuid(),
                Name = name,
                Category = category,
                OwnerId = owner.Id,
                CreatedAt = now.AddMinutes(-businessSequence * 17), // stagger for stable ORDER BY CreatedAt
                IsDeleted = false
            };
            businesses.Add(business);
            return business;
        }

        void AddServices(Business business, params (string Name, int DurationMinutes, decimal Price)[] defs)
        {
            foreach (var (name, durationMinutes, price) in defs)
            {
                services.Add(new Service
                {
                    Id = Guid.NewGuid(),
                    BusinessId = business.Id,
                    Name = name,
                    DurationMinutes = durationMinutes,
                    Price = price,
                    CreatedAt = business.CreatedAt,
                    IsDeleted = false
                });
            }
        }

        // Beauty — Noa Peretz (7 businesses, 20 services)
        AddServices(NewBusiness(users["Noa"], "Glow Nail Studio", "Beauty"),
            ("Manicure", 45, 90m), ("Pedicure", 60, 130m), ("Gel Polish", 45, 120m));

        AddServices(NewBusiness(users["Noa"], "Polished Beauty Bar", "Beauty"),
            ("Manicure", 45, 90m), ("Pedicure", 60, 130m), ("Nail Art Design", 60, 150m));

        AddServices(NewBusiness(users["Noa"], "Velvet Nails Lounge", "Beauty"),
            ("Manicure", 45, 90m), ("Eyelash Extensions", 90, 180m), ("Nail Art Design", 60, 150m));

        AddServices(NewBusiness(users["Noa"], "Radiant Nails & Spa", "Beauty"),
            ("Manicure", 45, 90m), ("Pedicure", 60, 130m), ("Gel Polish", 45, 120m),
            ("Eyelash Extensions", 90, 180m), ("Nail Art Design", 60, 150m));

        AddServices(NewBusiness(users["Noa"], "Chic Manicure House", "Beauty"),
            ("Manicure", 45, 90m));

        AddServices(NewBusiness(users["Noa"], "Pure Beauty Studio", "Beauty"),
            ("Pedicure", 60, 130m), ("Gel Polish", 45, 120m));

        AddServices(NewBusiness(users["Noa"], "Blossom Nail Salon", "Beauty"),
            ("Manicure", 45, 90m), ("Pedicure", 60, 130m), ("Nail Art Design", 60, 150m));

        // Sport — Itamar Katz (7 businesses, 16 services)
        AddServices(NewBusiness(users["Itamar"], "Iron Core Fitness", "Sport"),
            ("Personal Training Session", 60, 200m), ("Strength & Conditioning", 60, 180m));

        AddServices(NewBusiness(users["Itamar"], "Peak Performance Training", "Sport"),
            ("Personal Training Session", 60, 200m), ("Nutrition Consultation", 45, 250m));

        AddServices(NewBusiness(users["Itamar"], "FlexFit Studio", "Sport"),
            ("Group Fitness Class", 45, 70m), ("Yoga Session", 60, 90m));

        AddServices(NewBusiness(users["Itamar"], "Urban Strength Gym", "Sport"),
            ("Personal Training Session", 60, 200m), ("Strength & Conditioning", 60, 180m), ("Group Fitness Class", 45, 70m));

        AddServices(NewBusiness(users["Itamar"], "Zenith Personal Training", "Sport"),
            ("Personal Training Session", 60, 200m), ("Yoga Session", 60, 90m));

        AddServices(NewBusiness(users["Itamar"], "Vital Motion Fitness", "Sport"),
            ("Yoga Session", 60, 90m), ("Nutrition Consultation", 45, 250m));

        AddServices(NewBusiness(users["Itamar"], "Apex Athletics", "Sport"),
            ("Personal Training Session", 60, 200m), ("Group Fitness Class", 45, 70m), ("Strength & Conditioning", 60, 180m));

        // Repair / Auto — Maya Ben-David (6 businesses, 14 services)
        AddServices(NewBusiness(users["Maya"], "FixIt Home Services", "Repair"),
            ("Plumbing House Call", 60, 250m));

        AddServices(NewBusiness(users["Maya"], "ProFix Repairs", "Repair"),
            ("Electrical Repair", 90, 280m), ("Appliance Repair", 60, 220m), ("Tire Replacement", 45, 180m));

        AddServices(NewBusiness(users["Maya"], "QuickFix Handyman", "Repair"),
            ("General Handyman Service", 45, 120m), ("Furniture Assembly", 60, 150m));

        AddServices(NewBusiness(users["Maya"], "Reliable Repair Co.", "Repair"),
            ("Electrical Repair", 90, 280m), ("General Handyman Service", 45, 120m), ("Appliance Repair", 60, 220m));

        AddServices(NewBusiness(users["Maya"], "MasterTouch Repairs", "Repair"),
            ("Car Diagnostic", 60, 200m), ("Oil Change", 30, 150m), ("Tire Replacement", 45, 180m));

        AddServices(NewBusiness(users["Maya"], "HomeCraft Maintenance", "Repair"),
            ("Furniture Assembly", 60, 150m), ("General Handyman Service", 45, 120m));

        return (businesses, services);
    }

    private static List<Appointment> CreateAppointments(
        List<Business> businesses, List<Service> services, Dictionary<string, User> users)
    {
        var now = DateTimeOffset.UtcNow;
        var random = new Random(12345); // fixed seed for reproducible seed data

        var appointments = new List<Appointment>();
        var serviceBusyRanges = services.ToDictionary(s => s.Id, _ => new List<(DateTimeOffset Start, DateTimeOffset End)>());
        var clientBusyRanges = users.Values.ToDictionary(u => u.Id, _ => new List<(DateTimeOffset Start, DateTimeOffset End)>());

        var activeClientIds = new[] { users["Tal"].Id, users["Adi"].Id, users["Yael"].Id, users["Ori"].Id };

        bool Overlaps(Dictionary<Guid, List<(DateTimeOffset Start, DateTimeOffset End)>> ranges, Guid key,
            DateTimeOffset start, DateTimeOffset end)
            => ranges[key].Any(r => start < r.End && r.Start < end);

        Appointment? TryCreate(Service service, DateTimeOffset start, AppointmentStatus status, Guid? forcedClientId = null)
        {
            var end = start.AddMinutes(service.DurationMinutes);

            if (Overlaps(serviceBusyRanges, service.Id, start, end))
            {
                return null;
            }

            Guid clientId;
            if (forcedClientId.HasValue)
            {
                if (Overlaps(clientBusyRanges, forcedClientId.Value, start, end))
                {
                    return null;
                }

                clientId = forcedClientId.Value;
            }
            else
            {
                var candidate = activeClientIds
                    .OrderBy(_ => random.Next())
                    .FirstOrDefault(id => !Overlaps(clientBusyRanges, id, start, end));
                if (candidate == Guid.Empty)
                {
                    return null;
                }

                clientId = candidate;
            }

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                ServiceId = service.Id,
                ClientId = clientId,
                StartTime = start,
                EndTime = end,
                Status = status,
                CreatedAt = now,
                UpdatedAt = now
            };

            serviceBusyRanges[service.Id].Add((start, end));
            clientBusyRanges[clientId].Add((start, end));
            appointments.Add(appointment);
            return appointment;
        }

        // Every owner must have at least one upcoming appointment after Aug 7, plus a cancelled one after Aug 7.
        var ownerServices = services
            .Select(s => new { Service = s, OwnerId = businesses.First(b => b.Id == s.BusinessId).OwnerId })
            .GroupBy(x => x.OwnerId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Service).ToList());

        foreach (var servicesForOwner in ownerServices.Values)
        {
            var firstService = servicesForOwner[0];
            var secondService = servicesForOwner.Count > 1 ? servicesForOwner[1] : servicesForOwner[0];

            TryCreate(firstService, new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero), AppointmentStatus.Confirmed);
            TryCreate(secondService, new DateTimeOffset(2026, 8, 20, 14, 0, 0, TimeSpan.Zero), AppointmentStatus.Cancelled);
        }

        // General pool of appointments across all services, spread across the whole date range.
        var totalMinutesRange = (int)(RangeEnd - RangeStart).TotalMinutes;

        foreach (var service in services)
        {
            var count = random.Next(3, 6);
            for (var i = 0; i < count; i++)
            {
                for (var attempt = 0; attempt < 20; attempt++)
                {
                    var offsetMinutes = random.Next(0, totalMinutesRange);
                    var start = RangeStart.AddMinutes(offsetMinutes - offsetMinutes % 30);

                    var status = random.NextDouble() < 0.12
                        ? AppointmentStatus.Cancelled
                        : start < now ? AppointmentStatus.Confirmed : AppointmentStatus.Pending;

                    if (TryCreate(service, start, status) is not null)
                    {
                        break;
                    }
                }
            }
        }

        // Noga Regev is soft-deleted; all her appointments must be Cancelled regardless of when they were booked.
        foreach (var service in services.Take(5))
        {
            for (var attempt = 0; attempt < 20; attempt++)
            {
                var offsetMinutes = random.Next(0, totalMinutesRange);
                var start = RangeStart.AddMinutes(offsetMinutes - offsetMinutes % 30);

                if (TryCreate(service, start, AppointmentStatus.Cancelled, users["Noga"].Id) is not null)
                {
                    break;
                }
            }
        }

        return appointments;
    }
}
