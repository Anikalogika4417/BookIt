using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookIt.Data;
using BookIt.Models;
using BookIt.Models.DTOs.Request;
using BookIt.Models.Settings;
using BookIt.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BookIt.Services;

public class AuthService(AppDbContext context, JwtSettings jwtSettings, ILogger<AuthService> logger) : IAuthService
{
    public async Task<bool> RegisterAsync(AuthRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Started {Method}", nameof(RegisterAsync));

        var emailTaken = await context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailTaken)
        {
            return false;
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = request.Role,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Finished {Method}", nameof(RegisterAsync));
        return true;
    }

    public async Task<string?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Started {Method}", nameof(LoginAsync));

        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        logger.LogInformation("Finished {Method}", nameof(LoginAsync));
        return GenerateToken(user);
    }

    private string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
