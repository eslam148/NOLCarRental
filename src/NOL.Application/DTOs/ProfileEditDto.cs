using System.ComponentModel.DataAnnotations;
using NOL.Domain.Enums;

namespace NOL.Application.DTOs;

public class ProfileEditDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please provide a valid phone number")]
    [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Preferred language is required")]
    public Language PreferredLanguage { get; set; } = Language.English;
}
