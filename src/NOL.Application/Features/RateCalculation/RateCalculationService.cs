using NOL.Application.Common.Interfaces;
using NOL.Application.DTOs;

namespace NOL.Application.Features.RateCalculation;

public class RateCalculationService : IRateCalculationService
{
    private const int DaysInWeek = 7;
    private const int DaysInMonth = 30;

    public RateCalculationResponseDto CalculateOptimalRate(RateCalculationRequestDto request)
    {
        if (request.TotalDays <= 0)
            throw new ArgumentException("Total days must be greater than 0");

        var breakdown = CalculateOptimalBreakdown(
            request.TotalDays, 
            request.DailyRate, 
            request.WeeklyRate, 
            request.MonthlyRate);

        var totalCost = breakdown.MonthlyCost + breakdown.WeeklyCost + breakdown.DailyCost;

        return new RateCalculationResponseDto
        {
            TotalDays = request.TotalDays,
            TotalCost = totalCost,
            Breakdown = breakdown
        };
    }

    public RateCalculationResponseDto CalculateExtraRate(ExtraRateCalculationRequestDto request)
    {
        if (request.TotalDays <= 0 || request.Quantity <= 0)
            throw new ArgumentException("Total days and quantity must be greater than 0");

        var breakdown = CalculateOptimalBreakdown(
            request.TotalDays,
            request.DailyPrice,
            request.WeeklyPrice,
            request.MonthlyPrice);

        // Multiply by quantity
        breakdown.MonthlyCost *= request.Quantity;
        breakdown.WeeklyCost *= request.Quantity;
        breakdown.DailyCost *= request.Quantity;

        var totalCost = breakdown.MonthlyCost + breakdown.WeeklyCost + breakdown.DailyCost;

        return new RateCalculationResponseDto
        {
            TotalDays = request.TotalDays,
            TotalCost = totalCost,
            Breakdown = breakdown
        };
    }

    public OptimalRateCalculationDto CompareRateCalculations(RateCalculationRequestDto request)
    {
        var standardCost = CalculateSimpleRate(
            request.TotalDays, 
            request.DailyRate, 
            request.WeeklyRate, 
            request.MonthlyRate);

        var optimizedResult = CalculateOptimalRate(request);
        var optimizedCost = optimizedResult.TotalCost;

        var savings = standardCost - optimizedCost;

        return new OptimalRateCalculationDto
        {
            StandardCalculation = standardCost,
            OptimizedCalculation = optimizedCost,
            Savings = savings,
            IsOptimized = savings > 0,
            OptimizedBreakdown = optimizedResult.Breakdown
        };
    }

    public decimal CalculateSimpleRate(int totalDays, decimal dailyRate, decimal weeklyRate, decimal monthlyRate)
    {
        // Current implementation logic (simple tier-based)
        if (totalDays >= DaysInMonth)
        {
            var months = Math.Ceiling((decimal)totalDays / DaysInMonth);
            return monthlyRate * months;
        }
        else if (totalDays >= DaysInWeek)
        {
            var weeks = Math.Ceiling((decimal)totalDays / DaysInWeek);
            return weeklyRate * weeks;
        }
        else
        {
            return dailyRate * totalDays;
        }
    }

    private RateBreakdownDto CalculateOptimalBreakdown(int totalDays, decimal dailyRate, decimal weeklyRate, decimal monthlyRate)
    {
        var breakdown = new RateBreakdownDto();
        var remainingDays = totalDays;

        // Calculate monthly periods first (most cost-effective for long periods)
        if (remainingDays >= DaysInMonth)
        {
            breakdown.MonthlyPeriods = CalculateWholeUnits(remainingDays, DaysInMonth);
            breakdown.MonthlyCost = breakdown.MonthlyPeriods * monthlyRate;
            remainingDays -= breakdown.MonthlyPeriods * DaysInMonth;
        }

        // Calculate weekly periods for remaining days
        if (remainingDays >= DaysInWeek)
        {
            // Check if it's more cost-effective to use weekly vs daily rates
            var weeksNeeded = CalculateWholeUnits(remainingDays, DaysInWeek);
            var weeklyOptionCost = weeksNeeded * weeklyRate;
            var dailyOptionCost = remainingDays * dailyRate;

            if (weeklyOptionCost <= dailyOptionCost)
            {
                breakdown.WeeklyPeriods = weeksNeeded;
                breakdown.WeeklyCost = weeklyOptionCost;
                remainingDays -= weeksNeeded * DaysInWeek;
            }
        }

        // Remaining days are charged at daily rate
        if (remainingDays > 0)
        {
            breakdown.DailyPeriods = remainingDays;
            breakdown.DailyCost = remainingDays * dailyRate;
        }

        // Generate description
        breakdown.Description = GenerateBreakdownDescription(breakdown);

        return breakdown;
    }

    private int CalculateWholeUnits(int totalDays, int unitSize)
    {
        // Calculate whole units without using mod operator
        return totalDays / unitSize;
    }

    private string GenerateBreakdownDescription(RateBreakdownDto breakdown)
    {
        var parts = new List<string>();

        if (breakdown.MonthlyPeriods > 0)
            parts.Add($"{breakdown.MonthlyPeriods} month{(breakdown.MonthlyPeriods > 1 ? "s" : "")}");

        if (breakdown.WeeklyPeriods > 0)
            parts.Add($"{breakdown.WeeklyPeriods} week{(breakdown.WeeklyPeriods > 1 ? "s" : "")}");

        if (breakdown.DailyPeriods > 0)
            parts.Add($"{breakdown.DailyPeriods} day{(breakdown.DailyPeriods > 1 ? "s" : "")}");

        return parts.Count > 0 ? string.Join(" + ", parts) : "No periods";
    }
}
