using System.ComponentModel.DataAnnotations;

namespace NOL.Application.DTOs;

public class ContactUsDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Facebook { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string X { get; set; } = string.Empty;
    public string TikTok { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateContactUsDto
{
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please provide a valid phone number")]
    [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string Phone { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please provide a valid WhatsApp number")]
    [StringLength(50, ErrorMessage = "WhatsApp number cannot exceed 50 characters")]
    public string WhatsApp { get; set; } = string.Empty;

    [Url(ErrorMessage = "Please provide a valid Facebook URL")]
    [StringLength(255, ErrorMessage = "Facebook URL cannot exceed 255 characters")]
    public string Facebook { get; set; } = string.Empty;

    [Url(ErrorMessage = "Please provide a valid Instagram URL")]
    [StringLength(255, ErrorMessage = "Instagram URL cannot exceed 255 characters")]
    public string Instagram { get; set; } = string.Empty;

    [Url(ErrorMessage = "Please provide a valid X (Twitter) URL")]
    [StringLength(255, ErrorMessage = "X URL cannot exceed 255 characters")]
    public string X { get; set; } = string.Empty;

    [Url(ErrorMessage = "Please provide a valid TikTok URL")]
    [StringLength(255, ErrorMessage = "TikTok URL cannot exceed 255 characters")]
    public string TikTok { get; set; } = string.Empty;
}

public class UpdateContactUsDto
{
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Please provide a valid phone number")]
    [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string? Phone { get; set; }

    [Phone(ErrorMessage = "Please provide a valid WhatsApp number")]
    [StringLength(50, ErrorMessage = "WhatsApp number cannot exceed 50 characters")]
    public string? WhatsApp { get; set; }

    [Url(ErrorMessage = "Please provide a valid Facebook URL")]
    [StringLength(255, ErrorMessage = "Facebook URL cannot exceed 255 characters")]
    public string? Facebook { get; set; }

    [Url(ErrorMessage = "Please provide a valid Instagram URL")]
    [StringLength(255, ErrorMessage = "Instagram URL cannot exceed 255 characters")]
    public string? Instagram { get; set; }

    [Url(ErrorMessage = "Please provide a valid X (Twitter) URL")]
    [StringLength(255, ErrorMessage = "X URL cannot exceed 255 characters")]
    public string? X { get; set; }

    [Url(ErrorMessage = "Please provide a valid TikTok URL")]
    [StringLength(255, ErrorMessage = "TikTok URL cannot exceed 255 characters")]
    public string? TikTok { get; set; }

    public bool? IsActive { get; set; }
}

// Simplified DTO for public API (no sensitive admin fields)
public class PublicContactUsDto
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Facebook { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string X { get; set; } = string.Empty;
    public string TikTok { get; set; } = string.Empty;
}
