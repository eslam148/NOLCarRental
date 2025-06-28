namespace NOL.Domain.Entities;

public class Favorite
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public string UserId { get; set; } = string.Empty;
    public int CarId { get; set; }

    // Navigation Properties
    public ApplicationUser User { get; set; } = null!;
    public Car Car { get; set; } = null!;
} 