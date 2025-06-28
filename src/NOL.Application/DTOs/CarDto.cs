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
    public TransmissionType TransmissionType { get; set; }
    public FuelType FuelType { get; set; }
    public decimal DailyRate { get; set; }
    public decimal WeeklyRate { get; set; }
    public decimal MonthlyRate { get; set; }
    public CarStatus Status { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public int Mileage { get; set; }
    public CategoryDto Category { get; set; } = null!;
    public BranchDto Branch { get; set; } = null!;
} 