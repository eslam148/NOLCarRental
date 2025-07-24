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
    public string TransmissionType { get; set; } = string.Empty;
    public FuelType FuelType { get; set; }
    public decimal DailyPrice { get; set; }
    
    public CarStatus Status { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public int Mileage { get; set; }
    public CategoryDto Category { get; set; } = null!;
    public BranchDto Branch { get; set; } = null!;
    public bool IsFavorite { get; set; } = false; // Indicates if the car is a favorite for the user
} 