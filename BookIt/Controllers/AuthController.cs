using BookIt.Models.DTOs.Request;
using BookIt.Models.DTOs.Response;
using BookIt.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(AuthRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received request {Action}", nameof(RegisterAsync));

        var created = await authService.RegisterAsync(request, cancellationToken);
        if (!created)
        {
            return Conflict("Email is already registered.");
        }

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received request {Action}", nameof(LoginAsync));

        var token = await authService.LoginAsync(request, cancellationToken);
        if (token is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        return Ok(new AuthResponse { Token = token });
    }
}
