using NOL.Application.DTOs;
using NOL.Application.Features.RateCalculation;
using Xunit;

namespace NOL.Application.Tests.Features.RateCalculation;

public class RateCalculationServiceTests
{
    private readonly RateCalculationService _service;

    public RateCalculationServiceTests()
    {
        _service = new RateCalculationService();
    }

    [Theory]
    [InlineData(1, 100, 600, 2000, 100)] // 1 day = daily rate
    [InlineData(3, 100, 600, 2000, 300)] // 3 days = 3 * daily rate
    [InlineData(6, 100, 600, 2000, 600)] // 6 days = 6 * daily rate
    public void CalculateOptimalRate_ShortPeriods_UsesDailyRate(int days, decimal daily, decimal weekly, decimal monthly, decimal expected)
    {
        // Arrange
        var request = new RateCalculationRequestDto
        {
            TotalDays = days,
            DailyRate = daily,
            WeeklyRate = weekly,
            MonthlyRate = monthly
        };

        // Act
        var result = _service.CalculateOptimalRate(request);

        // Assert
        Assert.Equal(expected, result.TotalCost);
        Assert.Equal(days, result.Breakdown.DailyPeriods);
        Assert.Equal(0, result.Breakdown.WeeklyPeriods);
        Assert.Equal(0, result.Breakdown.MonthlyPeriods);
    }

    [Theory]
    [InlineData(7, 100, 600, 2000, 600)] // 1 week = weekly rate
    [InlineData(14, 100, 600, 2000, 1200)] // 2 weeks = 2 * weekly rate
    [InlineData(21, 100, 600, 2000, 1800)] // 3 weeks = 3 * weekly rate
    public void CalculateOptimalRate_WeeklyPeriods_UsesWeeklyRate(int days, decimal daily, decimal weekly, decimal monthly, decimal expected)
    {
        // Arrange
        var request = new RateCalculationRequestDto
        {
            TotalDays = days,
            DailyRate = daily,
            WeeklyRate = weekly,
            MonthlyRate = monthly
        };

        // Act
        var result = _service.CalculateOptimalRate(request);

        // Assert
        Assert.Equal(expected, result.TotalCost);
        Assert.Equal(days / 7, result.Breakdown.WeeklyPeriods);
        Assert.Equal(0, result.Breakdown.DailyPeriods);
        Assert.Equal(0, result.Breakdown.MonthlyPeriods);
    }

    [Theory]
    [InlineData(30, 100, 600, 2000, 2000)] // 1 month = monthly rate
    [InlineData(60, 100, 600, 2000, 4000)] // 2 months = 2 * monthly rate
    public void CalculateOptimalRate_MonthlyPeriods_UsesMonthlyRate(int days, decimal daily, decimal weekly, decimal monthly, decimal expected)
    {
        // Arrange
        var request = new RateCalculationRequestDto
        {
            TotalDays = days,
            DailyRate = daily,
            WeeklyRate = weekly,
            MonthlyRate = monthly
        };

        // Act
        var result = _service.CalculateOptimalRate(request);

        // Assert
        Assert.Equal(expected, result.TotalCost);
        Assert.Equal(days / 30, result.Breakdown.MonthlyPeriods);
        Assert.Equal(0, result.Breakdown.WeeklyPeriods);
        Assert.Equal(0, result.Breakdown.DailyPeriods);
    }

    [Fact]
    public void CalculateOptimalRate_MixedPeriods_OptimizesCorrectly()
    {
        // Arrange - 35 days should be 1 month + 5 days
        var request = new RateCalculationRequestDto
        {
            TotalDays = 35,
            DailyRate = 100,
            WeeklyRate = 600,
            MonthlyRate = 2000
        };

        // Act
        var result = _service.CalculateOptimalRate(request);

        // Assert
        Assert.Equal(2500, result.TotalCost); // 2000 (1 month) + 500 (5 days)
        Assert.Equal(1, result.Breakdown.MonthlyPeriods);
        Assert.Equal(0, result.Breakdown.WeeklyPeriods);
        Assert.Equal(5, result.Breakdown.DailyPeriods);
        Assert.Equal(2000, result.Breakdown.MonthlyCost);
        Assert.Equal(0, result.Breakdown.WeeklyCost);
        Assert.Equal(500, result.Breakdown.DailyCost);
    }

    [Fact]
    public void CalculateOptimalRate_ComplexMixedPeriods_OptimizesCorrectly()
    {
        // Arrange - 44 days should be 1 month + 2 weeks (14 days)
        var request = new RateCalculationRequestDto
        {
            TotalDays = 44,
            DailyRate = 100,
            WeeklyRate = 600,
            MonthlyRate = 2000
        };

        // Act
        var result = _service.CalculateOptimalRate(request);

        // Assert
        Assert.Equal(3200, result.TotalCost); // 2000 (1 month) + 1200 (2 weeks)
        Assert.Equal(1, result.Breakdown.MonthlyPeriods);
        Assert.Equal(2, result.Breakdown.WeeklyPeriods);
        Assert.Equal(0, result.Breakdown.DailyPeriods);
    }

    [Fact]
    public void CalculateExtraRate_WithQuantity_MultipliesCorrectly()
    {
        // Arrange
        var request = new ExtraRateCalculationRequestDto
        {
            TotalDays = 7,
            Quantity = 2,
            DailyPrice = 50,
            WeeklyPrice = 300,
            MonthlyPrice = 1000
        };

        // Act
        var result = _service.CalculateExtraRate(request);

        // Assert
        Assert.Equal(600, result.TotalCost); // 300 (weekly) * 2 (quantity)
    }

    [Fact]
    public void CompareRateCalculations_ShowsOptimization()
    {
        // Arrange - 35 days: simple would charge 2 months, optimized charges 1 month + 5 days
        var request = new RateCalculationRequestDto
        {
            TotalDays = 35,
            DailyRate = 100,
            WeeklyRate = 600,
            MonthlyRate = 2000
        };

        // Act
        var result = _service.CompareRateCalculations(request);

        // Assert
        Assert.Equal(4000, result.StandardCalculation); // 2 months
        Assert.Equal(2500, result.OptimizedCalculation); // 1 month + 5 days
        Assert.Equal(1500, result.Savings);
        Assert.True(result.IsOptimized);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void CalculateOptimalRate_InvalidDays_ThrowsException(int invalidDays)
    {
        // Arrange
        var request = new RateCalculationRequestDto
        {
            TotalDays = invalidDays,
            DailyRate = 100,
            WeeklyRate = 600,
            MonthlyRate = 2000
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.CalculateOptimalRate(request));
    }

    [Fact]
    public void CalculateSimpleRate_MatchesCurrentLogic()
    {
        // Test that simple rate calculation matches the original BookingService logic
        
        // 35 days should use monthly rate (2 months)
        var result = _service.CalculateSimpleRate(35, 100, 600, 2000);
        Assert.Equal(4000, result); // Math.Ceiling(35/30) * 2000 = 2 * 2000

        // 10 days should use weekly rate (2 weeks)
        result = _service.CalculateSimpleRate(10, 100, 600, 2000);
        Assert.Equal(1200, result); // Math.Ceiling(10/7) * 600 = 2 * 600

        // 5 days should use daily rate
        result = _service.CalculateSimpleRate(5, 100, 600, 2000);
        Assert.Equal(500, result); // 5 * 100
    }

    [Fact]
    public void CalculateOptimalRate_GeneratesCorrectDescription()
    {
        // Arrange
        var request = new RateCalculationRequestDto
        {
            TotalDays = 44, // 1 month + 2 weeks
            DailyRate = 100,
            WeeklyRate = 600,
            MonthlyRate = 2000
        };

        // Act
        var result = _service.CalculateOptimalRate(request);

        // Assert
        Assert.Equal("1 month + 2 weeks", result.Breakdown.Description);
    }
}
