using NOL.Application.DTOs;

namespace NOL.Application.Common.Interfaces;

public interface IRateCalculationService
{
    /// <summary>
    /// Calculate the optimal rental cost for a car based on daily, weekly, and monthly rates
    /// </summary>
    /// <param name="request">Rate calculation request containing days and rates</param>
    /// <returns>Optimal rate calculation with breakdown</returns>
    RateCalculationResponseDto CalculateOptimalRate(RateCalculationRequestDto request);
    
    /// <summary>
    /// Calculate the cost for extras based on daily, weekly, and monthly prices
    /// </summary>
    /// <param name="request">Extra rate calculation request</param>
    /// <returns>Rate calculation response with breakdown</returns>
    RateCalculationResponseDto CalculateExtraRate(ExtraRateCalculationRequestDto request);
    
    /// <summary>
    /// Compare standard vs optimized rate calculation
    /// </summary>
    /// <param name="request">Rate calculation request</param>
    /// <returns>Comparison between standard and optimized calculations</returns>
    OptimalRateCalculationDto CompareRateCalculations(RateCalculationRequestDto request);
    
    /// <summary>
    /// Calculate simple rate without optimization (current implementation)
    /// </summary>
    /// <param name="totalDays">Total rental days</param>
    /// <param name="dailyRate">Daily rate</param>
    /// <param name="weeklyRate">Weekly rate</param>
    /// <param name="monthlyRate">Monthly rate</param>
    /// <returns>Simple rate calculation</returns>
    decimal CalculateSimpleRate(int totalDays, decimal dailyRate, decimal weeklyRate, decimal monthlyRate);
}
