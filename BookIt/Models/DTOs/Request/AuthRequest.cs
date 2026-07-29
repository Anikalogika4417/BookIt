using System.ComponentModel.DataAnnotations;
using BookIt.Models.Enums;

namespace BookIt.Models.DTOs.Request;

public class AuthRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}
