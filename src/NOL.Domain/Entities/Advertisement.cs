using NOL.Domain.Enums;

namespace NOL.Domain.Entities;

public class Advertisement
{
    public int Id { get; set; }
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ImageUrl { get; set; }
    public AdvertisementType Type { get; set; } = AdvertisementType.Special;
    public AdvertisementStatus Status { get; set; } = AdvertisementStatus.Active;
    public int ViewCount { get; set; } = 0;
    public int ClickCount { get; set; } = 0;
    public bool IsFeatured { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int? CarId { get; set; } // Nullable for category-wide ads
    public int? CategoryId { get; set; } // Nullable for car-specific ads
    public string? CreatedByUserId { get; set; } // Admin who created the ad

    // Navigation Properties
    public virtual Car? Car { get; set; }
    public virtual Category? Category { get; set; }
    public virtual ApplicationUser? CreatedByUser { get; set; }
} 