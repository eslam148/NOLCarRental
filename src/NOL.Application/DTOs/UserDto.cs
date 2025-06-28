using NOL.Domain.Enums;

namespace NOL.Application.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int loyaltyPoints { get; set; } = 0;
    public Language PreferredLanguage { get; set; }
    public UserRole UserRole { get; set; }
    public bool emailVerified { get; set; } = false;
} 