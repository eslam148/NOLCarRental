using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.DTOs;
using NOL.Domain.Enums;
using System.Security.Claims;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoyaltyPointsController : ControllerBase
{
    private readonly ILoyaltyPointService _loyaltyPointService;

    public LoyaltyPointsController(ILoyaltyPointService loyaltyPointService)
    {
        _loyaltyPointService = loyaltyPointService;
    }

    /// <summary>
    /// Get loyalty point summary for the authenticated user
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetLoyaltyPointSummary()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var result = await _loyaltyPointService.GetUserLoyaltyPointSummaryAsync(userId);
        
        if (result.Succeeded)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get transaction history for the authenticated user
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _loyaltyPointService.GetUserTransactionsAsync(userId, page, pageSize);
        
        if (result.Succeeded)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Redeem loyalty points for discount
    /// </summary>
    [HttpPost("redeem")]
    public async Task<IActionResult> RedeemPoints([FromBody] RedeemPointsDto redeemDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _loyaltyPointService.RedeemPointsAsync(userId, redeemDto);
        
        if (result.Succeeded)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Award points to a user (Admin/Manager only)
    /// </summary>
    [HttpPost("award")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AwardPoints([FromBody] AwardPointsDto awardDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _loyaltyPointService.AwardPointsAsync(awardDto);
        
        if (result.Succeeded)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Calculate discount amount for given points
    /// </summary>
    [HttpGet("calculate-discount/{points}")]
    [AllowAnonymous]
    public IActionResult CalculateDiscount(int points)
    {
        if (points < 0)
        {
            return BadRequest(new { message = "Points must be a positive number" });
        }

        var discount = _loyaltyPointService.CalculateDiscountForPoints(points);
        return Ok(new { Points = points, DiscountAmount = discount });
    }

    /// <summary>
    /// Calculate points that would be earned for given amount
    /// </summary>
    [HttpGet("calculate-points/{amount}")]
    [AllowAnonymous]
    public IActionResult CalculatePoints(decimal amount)
    {
        if (amount < 0)
        {
            return BadRequest(new { message = "Amount must be a positive number" });
        }

        var points = _loyaltyPointService.CalculatePointsForAmount(amount);
        return Ok(new { Amount = amount, PointsEarned = points });
    }

    /// <summary>
    /// Process points for a completed booking (Internal use)
    /// </summary>
    [HttpPost("process-booking/{bookingId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ProcessBookingPoints(int bookingId, [FromBody] decimal bookingAmount)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        if (bookingAmount <= 0)
        {
            return BadRequest(new { message = "Booking amount must be greater than zero" });
        }

        var result = await _loyaltyPointService.ProcessBookingPointsAsync(userId, bookingId, bookingAmount);
        
        if (result.Succeeded)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get loyalty point system information
    /// </summary>
    [HttpGet("info")]
    [AllowAnonymous]
    public IActionResult GetSystemInfo()
    {
        return Ok(new
        {
            PointsPerDollar = 1,
            PointValue = 0.01m,
            MinimumRedemption = 100,
            PointsExpiryMonths = 24,
            BonusPoints = new
            {
                Registration = 100,
                Review = 50,
                Referral = 500
            }
        });
    }

    /// <summary>
    /// Award welcome bonus to new user (Internal use)
    /// </summary>
    [HttpPost("welcome-bonus/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AwardWelcomeBonus(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { message = "User ID is required" });
        }

        var awardDto = new AwardPointsDto
        {
            UserId = userId,
            Points = 100,
            EarnReason = LoyaltyPointEarnReason.Registration,
            Description = "Welcome bonus for new registration"
        };

        var result = await _loyaltyPointService.AwardPointsAsync(awardDto);
        
        if (result.Succeeded)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Award review bonus (Internal use)
    /// </summary>
    [HttpPost("review-bonus")]
    [Authorize]
    public async Task<IActionResult> AwardReviewBonus([FromBody] int bookingId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var awardDto = new AwardPointsDto
        {
            UserId = userId,
            Points = 50,
            EarnReason = LoyaltyPointEarnReason.Review,
            BookingId = bookingId,
            Description = "Points for writing a review"
        };

        var result = await _loyaltyPointService.AwardPointsAsync(awardDto);
        
        if (result.Succeeded)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Expire old points (Background job endpoint)
    /// </summary>
    [HttpPost("expire-old-points")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExpireOldPoints()
    {
        var result = await _loyaltyPointService.ExpireOldPointsAsync();
        
        if (result.Succeeded)
            return Ok(result);
        
        return BadRequest(result);
    }
} 