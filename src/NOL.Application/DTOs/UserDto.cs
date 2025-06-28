using NOL.Domain.Enums;

namespace NOL.Application.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Language PreferredLanguage { get; set; }
    public UserRole UserRole { get; set; }
} 