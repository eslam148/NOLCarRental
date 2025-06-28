using NOL.Domain.Enums;

namespace NOL.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string MessageAr { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }

    // Foreign Keys
    public string UserId { get; set; } = string.Empty;

    // Navigation Properties
    public ApplicationUser User { get; set; } = null!;
} 