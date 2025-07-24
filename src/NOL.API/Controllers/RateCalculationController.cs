using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.DTOs;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RateCalculationController : ControllerBase
{
    private readonly IRateCalculationService _rateCalculationService;

    public RateCalculationController(IRateCalculationService rateCalculationService)
    {
        _rateCalculationService = rateCalculationService;
    }

    /// <summary>
    /// Calculate optimal rental rate for given parameters
    /// </summary>
    [HttpPost("calculate-optimal")]
    [AllowAnonymous]
    public IActionResult CalculateOptimalRate([FromBody] RateCalculationRequestDto request)
    {
        try
        {
            var result = _rateCalculationService.CalculateOptimalRate(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Calculate rate for extras with quantity
    /// </summary>
    [HttpPost("calculate-extra")]
    [AllowAnonymous]
    public IActionResult CalculateExtraRate([FromBody] ExtraRateCalculationRequestDto request)
    {
        try
        {
            var result = _rateCalculationService.CalculateExtraRate(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Compare standard vs optimized rate calculation
    /// </summary>
    [HttpPost("compare-rates")]
    [AllowAnonymous]
    public IActionResult CompareRateCalculations([FromBody] RateCalculationRequestDto request)
    {
        try
        {
            var result = _rateCalculationService.CompareRateCalculations(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Calculate simple rate (current implementation for comparison)
    /// </summary>
    [HttpGet("calculate-simple")]
    [AllowAnonymous]
    public IActionResult CalculateSimpleRate(
        [FromQuery] int totalDays,
        [FromQuery] decimal dailyRate,
        [FromQuery] decimal weeklyRate,
        [FromQuery] decimal monthlyRate)
    {
        try
        {
            if (totalDays <= 0)
                return BadRequest(new { message = "Total days must be greater than 0" });

            var result = _rateCalculationService.CalculateSimpleRate(totalDays, dailyRate, weeklyRate, monthlyRate);
            return Ok(new { TotalDays = totalDays, TotalCost = result, Method = "Simple" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get rate calculation examples for demonstration
    /// </summary>
    [HttpGet("examples")]
    [AllowAnonymous]
    public IActionResult GetExamples()
    {
        var examples = new[]
        {
            new
            {
                Scenario = "35 days rental",
                Request = new RateCalculationRequestDto
                {
                    TotalDays = 35,
                    DailyRate = 100,
                    WeeklyRate = 600,
                    MonthlyRate = 2000
                },
                Description = "Shows optimization: 1 month + 5 days vs 2 months"
            },
            new
            {
                Scenario = "44 days rental",
                Request = new RateCalculationRequestDto
                {
                    TotalDays = 44,
                    DailyRate = 100,
                    WeeklyRate = 600,
                    MonthlyRate = 2000
                },
                Description = "Shows optimization: 1 month + 2 weeks vs 2 months"
            },
            new
            {
                Scenario = "10 days rental",
                Request = new RateCalculationRequestDto
                {
                    TotalDays = 10,
                    DailyRate = 100,
                    WeeklyRate = 600,
                    MonthlyRate = 2000
                },
                Description = "Shows weekly vs daily rate optimization"
            }
        };

        return Ok(examples);
    }
}
