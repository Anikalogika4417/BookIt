# BookIt — Appointment Booking API

Backend for BookIt, a platform where business owners (barbers, physiotherapists, tutors...) list their services and clients book appointments with them.

## Tech Stack

- C# / .NET 8 Web API
- PostgreSQL via EF Core (Npgsql provider), snake_case naming convention (`EFCore.NamingConventions`)
- JWT authentication (HMAC-SHA256)
- BCrypt for password hashing
- xUnit + EF Core InMemory for unit tests

## Setup & Run

1. Start Postgres:

```bash
   docker run --name bookit-db -e POSTGRES_PASSWORD=bookit -e POSTGRES_DB=bookit -p 5432:5432 -d postgres:16
```

2. Configure the JWT signing key.

   `appsettings.Development.json` is git-ignored (as is standard practice for files that may contain secrets), so it won't be present after cloning. Create it manually at `BookIt/appsettings.Development.json` with the content below — this is a working configuration provided for convenience so no additional setup is needed. (Note: in a real production deployment, secrets like the JWT key would instead be moved to `dotnet user-secrets` or an environment variable and never committed to source control at all — it's included here as plain JSON only to make this take-home assignment easier to run.)

```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=bookit;Username=postgres;Password=bookit"
     },
     "Jwt": {
       "Key": "cVbBFhQHUSYhsvGx4Wz4mPxxw9Yb9ImrgZjlr3qsD68xx45gWzT1ytMhb5QYuaPo",
       "Issuer": "BookIt",
       "Audience": "BookItClient",
       "ExpiryMinutes": 60
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning",
         "Microsoft.EntityFrameworkCore.Database.Command": "Information"
       }
     },
     "AllowedHosts": "*"
   }
```
3. Apply migrations (creates schema):

```bash
   dotnet ef database update --project BookIt
```

4. Run the API:

```bash
   dotnet run --project BookIt
```

The app seeds the database automatically on first run (only if the `users` table is empty).

Swagger UI opens at `https://localhost:7270/swagger` (or `http://localhost:5052/swagger`).

## Seeded Login Credentials

| Role          | Email                          | Password                          | Notes                                      |
|---------------|----------------------------------|-------------------------------------|---------------------------------------------|
| BusinessOwner | noa.peretz@bookitdemo.com       | noa.peretz@bookitdemo.com_pass      | owns 7 Beauty businesses                     |
| BusinessOwner | itamar.katz@bookitdemo.com      | itamar.katz@bookitdemo.com_pass     | owns 7 Sport businesses                      |
| BusinessOwner | maya.bendavid@bookitdemo.com    | maya.bendavid@bookitdemo.com_pass   | owns 6 Repair businesses                     |
| Client        | tal.avidan@example.com          | tal.avidan@example.com_pass         |                                               |
| Client        | adi.nagar@example.com           | adi.nagar@example.com_pass          |                                               |
| Client        | yael.segal@example.com          | ori.harel@example.com               | intentionally shares Ori's password value    |
| Client        | ori.harel@example.com           | ori.harel@example.com               |                                               |
| Client        | noga.regev@example.com          | noga.regev@example.com_pass         |                                               |

Seed data: ~20 businesses, ~50 services, ~200 appointments across a range of past/future dates and statuses.

## Architecture & DI

Controllers → Services (behind interfaces, e.g. `IAppointmentService`) → EF Core `DbContext`. All I/O is async end-to-end.

**DI lifetimes:** `AppDbContext` and all services (`IAuthService`, `IBusinessService`, `IAppointmentService`) are registered as **Scoped**. 
A `DbContext` is not thread-safe and represents a single unit of work, so it must live no longer than one HTTP request — Scoped gives exactly one instance per request, shared consistently across everything resolved within that request.

## API Overview

- `POST /api/auth/register`, `POST /api/auth/login` — JWT auth. All other endpoints require a valid Bearer token.
- `GET /api/businesses` — paginated, optional `category` / `search` filters, returns `TotalCount`.
- `POST /api/appointments` — Client books an appointment.
- `GET /api/appointments?from=&to=&status=` — returns only the caller's own appointments (Client: own bookings; BusinessOwner: appointments across all businesses they own). `from`/`to` are `DateTimeOffset` (ISO-8601, e.g. `2026-08-01T00:00:00Z`); `from` must not be later than `to` (400 otherwise).
- `GET /api/businesses/{id}/summary` — owner-only (403 for anyone else); per-service count of upcoming non-cancelled appointments and expected revenue.

Request validation (email format, required fields, `StartTime` must be in the future) is done via DataAnnotations; invalid input returns the framework's standard 400 `ValidationProblemDetails`.

## Middleware

- **Global exception handling** — unhandled exceptions return a generic 500 `ProblemDetails`; full details are logged server-side, never sent to the client.
- **Request logging** — logs method, path, status code and duration for every request;

## Tests

```bash
dotnet test Tests/Tests.csproj
```

9 unit tests cover `AppointmentService` (overlap detection for both service and client, boundary/back-to-back timing, cancelled appointments not blocking, and Client vs. BusinessOwner visibility across multiple owned businesses), using EF Core InMemory.

## Postman Collection

Import `BookIt.postman_collection.json` (and `BookIt.postman_environment.json`) from the repo root into Postman to get all endpoints pre-configured
