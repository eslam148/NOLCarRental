using Microsoft.AspNetCore.Identity;
using NOL.Domain.Enums;

namespace NOL.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole UserRole { get; set; } = UserRole.Customer;
    public Language PreferredLanguage { get; set; } = Language.English;
    public bool IsActive { get; set; } = true;
    public int TotalLoyaltyPoints { get; set; } = 0;
    public int AvailableLoyaltyPoints { get; set; } = 0;
    public int LifetimePointsEarned { get; set; } = 0;
    public int LifetimePointsRedeemed { get; set; } = 0;
    public DateTime? LastPointsEarnedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<LoyaltyPointTransaction> LoyaltyPointTransactions { get; set; } = new List<LoyaltyPointTransaction>();
} 