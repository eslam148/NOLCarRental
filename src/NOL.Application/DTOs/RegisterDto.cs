using System.ComponentModel.DataAnnotations;
using NOL.Domain.Enums;

namespace NOL.Application.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = null;

    [Required]
    public string FullName { get; set; } = string.Empty;

     

    public Language PreferredLanguage { get; set; } = Language.Arabic;
} 