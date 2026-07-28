using System.Security.Claims;
using BookIt.Models.Enums;

namespace BookIt.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static UserRole GetRole(this ClaimsPrincipal user)
        => Enum.Parse<UserRole>(user.FindFirstValue(ClaimTypes.Role)!);
}
