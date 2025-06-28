using NOL.Domain.Enums;

namespace NOL.Application.DTOs;

public class AdvertisementDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ImageUrl { get; set; }
    public AdvertisementType Type { get; set; }
    public AdvertisementStatus Status { get; set; }
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public CarDto? Car { get; set; }
    public CategoryDto? Category { get; set; }
}

public class CreateAdvertisementDto
{
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
    public AdvertisementType Type { get; set; }
    public bool IsFeatured { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public int? CarId { get; set; }
    public int? CategoryId { get; set; }
}

public class UpdateAdvertisementDto
{
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
    public AdvertisementType Type { get; set; }
    public AdvertisementStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public int? CarId { get; set; }
    public int? CategoryId { get; set; }
} 