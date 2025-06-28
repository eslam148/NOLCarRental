namespace NOL.Domain.Entities;

public class Review
{
    public int Id { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Foreign Keys
    public string UserId { get; set; } = string.Empty;
    public int CarId { get; set; }

    // Navigation Properties
    public ApplicationUser User { get; set; } = null!;
    public Car Car { get; set; } = null!;
} 