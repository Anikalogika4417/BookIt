using BookIt.Models.DTOs.Request;

namespace BookIt.Services.Interfaces;

public interface IAuthService
{
    // Returns null when the email is already registered.
    Task<string?> RegisterAsync(AuthRequest request);

    // Returns null when the credentials are invalid.
    Task<string?> LoginAsync(LoginRequest request);
}
