using BookIt.Models.DTOs.Request;

namespace BookIt.Services.Interfaces;

public interface IAuthService
{
    // Returns false when the email is already registered.
    Task<bool> RegisterAsync(AuthRequest request);

    // Returns null when the credentials are invalid.
    Task<string?> LoginAsync(LoginRequest request);
}
