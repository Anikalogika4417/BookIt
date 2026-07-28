using BookIt.Models.DTOs.Request;
using BookIt.Models.DTOs.Response;
using BookIt.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRequest request)
    {
        var created = await authService.RegisterAsync(request);
        if (!created)
        {
            return Conflict("Email is already registered.");
        }

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await authService.LoginAsync(request);
        if (token is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        return Ok(new AuthResponse { Token = token });
    }
}
