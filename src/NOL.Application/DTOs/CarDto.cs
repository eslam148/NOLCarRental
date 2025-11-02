using System.ComponentModel.DataAnnotations;
using NOL.Domain.Enums;

namespace NOL.Application.DTOs;

public class CarDto
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public int NumberOfDoors { get; set; }
    public int MaxSpeed { get; set; } // Maximum speed in km/h
    public string Engine { get; set; } = string.Empty; // Engine specifications
    public string TransmissionType { get; set; } = string.Empty;
    public FuelType FuelType { get; set; }
    public decimal DailyPrice { get; set; }
    public decimal WeeklyPrice { get; set; }
    public decimal MonthlyPrice { get; set; }

    public CarStatus StatusName{ get; set; }
    public string Status { get; set; }
    
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public int Mileage { get; set; }
    public CategoryDto Category { get; set; } = null!;
    public BranchDto Branch { get; set; } = null!;
    public bool IsFavorite { get; set; } = false; // Indicates if the car is a favorite for the user

    public decimal AvrageRate { get; set; } = 0;

    public int RateCount { get; set; }
    public string PlateNumber {  get; set; } = string.Empty!;
}

public class CreateCarDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string BrandAr { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string BrandEn { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string ModelAr { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string ModelEn { get; set; } = string.Empty;

    [Required]
    [Range(1900, 2030)]
    public int Year { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string ColorAr { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string ColorEn { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string PlateNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, 20)]
    public int SeatingCapacity { get; set; }

    [Required]
    [Range(2, 6)]
    public int NumberOfDoors { get; set; }

    [Required]
    [Range(80, 400)]
    public int MaxSpeed { get; set; } // Maximum speed in km/h

    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Engine { get; set; } = string.Empty; // Engine specifications

    [Required]
    public TransmissionType TransmissionType { get; set; }

    [Required]
    public FuelType FuelType { get; set; }

    [Required]
    [Range(0.01, 10000)]
    public decimal DailyRate { get; set; }

    [Required]
    [Range(0.01, 50000)]
    public decimal WeeklyRate { get; set; }

    [Required]
    [Range(0.01, 200000)]
    public decimal MonthlyRate { get; set; }

    public CarStatus Status { get; set; } = CarStatus.Available;

    [Url]
    public string? ImageUrl { get; set; }

    [StringLength(1000)]
    public string? DescriptionAr { get; set; }

    [StringLength(1000)]
    public string? DescriptionEn { get; set; }

    [Range(0, 1000000)]
    public int Mileage { get; set; }

    public string? Features { get; set; } // JSON string of features

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int BranchId { get; set; }
}

public class UpdateCarDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? BrandAr { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string? BrandEn { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string? ModelAr { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string? ModelEn { get; set; }

    [Range(1900, 2030)]
    public int? Year { get; set; }

    [StringLength(50, MinimumLength = 2)]
    public string? ColorAr { get; set; }

    [StringLength(50, MinimumLength = 2)]
    public string? ColorEn { get; set; }

    [StringLength(20, MinimumLength = 3)]
    public string? PlateNumber { get; set; }

    [Range(1, 20)]
    public int? SeatingCapacity { get; set; }

    [Range(2, 6)]
    public int? NumberOfDoors { get; set; }

    [Range(80, 400)]
    public int? MaxSpeed { get; set; } // Maximum speed in km/h

    [StringLength(200, MinimumLength = 3)]
    public string? Engine { get; set; } // Engine specifications

    public TransmissionType? TransmissionType { get; set; }

    public FuelType? FuelType { get; set; }

    public CarStatus? Status { get; set; }

    [Url]
    public string? ImageUrl { get; set; }

    [StringLength(1000)]
    public string? DescriptionAr { get; set; }

    [StringLength(1000)]
    public string? DescriptionEn { get; set; }

    [Range(0, 1000000)]
    public int? Mileage { get; set; }

    public string? Features { get; set; } // JSON string of features

    public int? CategoryId { get; set; }

    public int? BranchId { get; set; }
}

public class UpdateCarRatesDto
{
    [Required]
    [Range(0.01, 10000, ErrorMessage = "Daily rate must be between 0.01 and 10000")]
    public decimal DailyRate { get; set; }

    [Required]
    [Range(0.01, 50000, ErrorMessage = "Weekly rate must be between 0.01 and 50000")]
    public decimal WeeklyRate { get; set; }

    [Required]
    [Range(0.01, 200000, ErrorMessage = "Monthly rate must be between 0.01 and 200000")]
    public decimal MonthlyRate { get; set; }
}

public class CarRatesDto
{
    public int CarId { get; set; }
    public string CarName { get; set; } = string.Empty;
    public decimal DailyRate { get; set; }
    public decimal WeeklyRate { get; set; }
    public decimal MonthlyRate { get; set; }
    public DateTime LastUpdated { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}

public class BulkUpdateRatesDto
{
    [Required]
    public List<CarRateUpdateItem> CarRates { get; set; } = new();
}

public class CarRateUpdateItem
{
    [Required]
    public int CarId { get; set; }

    [Required]
    [Range(0.01, 10000)]
    public decimal DailyRate { get; set; }

    [Required]
    [Range(0.01, 50000)]
    public decimal WeeklyRate { get; set; }

    [Required]
    [Range(0.01, 200000)]
    public decimal MonthlyRate { get; set; }
}