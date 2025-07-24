using System.ComponentModel.DataAnnotations;

namespace NOL.Application.DTOs;

public class RateCalculationRequestDto
{
    [Required]
    public int TotalDays { get; set; }
    
    [Required]
    public decimal DailyRate { get; set; }
    
    [Required]
    public decimal WeeklyRate { get; set; }
    
    [Required]
    public decimal MonthlyRate { get; set; }
}

public class ExtraRateCalculationRequestDto
{
    [Required]
    public int TotalDays { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public decimal DailyPrice { get; set; }
    
    [Required]
    public decimal WeeklyPrice { get; set; }
    
    [Required]
    public decimal MonthlyPrice { get; set; }
}

public class RateCalculationResponseDto
{
    public int TotalDays { get; set; }
    public decimal TotalCost { get; set; }
    public RateBreakdownDto Breakdown { get; set; } = new();
}

public class RateBreakdownDto
{
    public int MonthlyPeriods { get; set; }
    public decimal MonthlyCost { get; set; }
    
    public int WeeklyPeriods { get; set; }
    public decimal WeeklyCost { get; set; }
    
    public int DailyPeriods { get; set; }
    public decimal DailyCost { get; set; }
    
    public string Description { get; set; } = string.Empty;
}

public class OptimalRateCalculationDto
{
    public decimal StandardCalculation { get; set; }
    public decimal OptimizedCalculation { get; set; }
    public decimal Savings { get; set; }
    public bool IsOptimized { get; set; }
    public RateBreakdownDto OptimizedBreakdown { get; set; } = new();
}
