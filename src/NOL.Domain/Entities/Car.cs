using NOL.Domain.Enums;

namespace NOL.Domain.Entities;

public class Car
{
    public int Id { get; set; }
    public string BrandAr { get; set; } = string.Empty;
    public string BrandEn { get; set; } = string.Empty;
    public string ModelAr { get; set; } = string.Empty;
    public string ModelEn { get; set; } = string.Empty;
    public int Year { get; set; }
    public string ColorAr { get; set; } = string.Empty;
    public string ColorEn { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public int NumberOfDoors { get; set; }
    public int MaxSpeed { get; set; } // Maximum speed in km/h
    public string Engine { get; set; } = string.Empty; // Engine specifications
    public TransmissionType TransmissionType { get; set; }
    public FuelType FuelType { get; set; }
    public decimal DailyRate { get; set; }
    public decimal WeeklyRate { get; set; }
    public decimal MonthlyRate { get; set; }
    public CarStatus Status { get; set; } = CarStatus.Available;
    public string? ImageUrl { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public int Mileage { get; set; }
    public string? Features { get; set; } // JSON string of features
    public bool IsActive { get; set; } = true;

    // Rating properties
    public decimal AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int CategoryId { get; set; }
    public int BranchId { get; set; }

    // Navigation Properties
    public virtual Category Category { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
} 