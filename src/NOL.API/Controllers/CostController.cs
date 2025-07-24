using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.DTOs;
using System.Security.Claims;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CostController : ControllerBase
{
    private readonly ICarRepository _carRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILoyaltyPointService _loyaltyPointService;
    private readonly IBranchRepository _branchRepository;

    // Configuration constants for Saudi Arabia
    private const decimal TAX_PERCENTAGE = 15.0m; // 15% VAT
    private const decimal INSURANCE_PERCENTAGE = 5.0m; // 5% insurance fee
    private const decimal DELIVERY_FEE = 50.0m; // Fixed delivery fee in SAR
    private const int LOYALTY_POINTS_PER_SAR = 1; // 1 point per SAR spent
    private const decimal LOYALTY_POINT_VALUE = 0.1m; // 1 point = 0.1 SAR
    private const decimal MAX_LOYALTY_REDEMPTION_PERCENTAGE = 50.0m; // Max 50% of booking

    public CostController(
        ICarRepository carRepository,
        IBookingRepository bookingRepository,
        ILoyaltyPointService loyaltyPointService,
        IBranchRepository branchRepository)
    {
        _carRepository = carRepository;
        _bookingRepository = bookingRepository;
        _loyaltyPointService = loyaltyPointService;
        _branchRepository = branchRepository;
    }

    /// <summary>
    /// Calculate comprehensive booking cost with detailed breakdown
    /// </summary>
    /// <param name="request">Booking cost calculation request</param>
    /// <returns>Complete cost breakdown with all fees, taxes, and discounts</returns>
    [HttpPost("calculate-cost")]
    [AllowAnonymous]
    public async Task<IActionResult> CalculateBookingCost([FromBody] BookingCostCalculationRequestDto request)
    {
        try
        {
            // Input validation
            var validationResult = ValidateRequest(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { 
                    success = false, 
                    message = validationResult.ErrorMessage,
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // Get authenticated user ID if available
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                request.UserId = userId;
            }

            // Get car details
            var car = await _carRepository.GetByIdAsync(request.CarId);
            if (car == null)
            {
                return NotFound(new { 
                    success = false, 
                    message = "Car not found" 
                });
            }

            // Check car availability
            var isAvailable = await _bookingRepository.IsCarAvailableAsync(
                request.CarId, 
                request.StartDate, 
                request.EndDate);

            if (!isAvailable)
            {
                return Ok(new BookingCostCalculationResponseDto
                {
                    CarId = request.CarId,
                    CarName = $"{car.BrandEn} {car.ModelEn}",
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsAvailable = false,
                    UnavailabilityReason = "Car is not available for the selected dates",
                    FinalAmount = 0,
                    Currency = "SAR"
                });
            }

            // Calculate rental period
            var totalDays = (int)Math.Ceiling((request.EndDate - request.StartDate).TotalDays);
            var totalHours = (int)Math.Ceiling((request.EndDate - request.StartDate).TotalHours);

            // Calculate base cost
            var dailyRate = car.DailyRate;
            var baseCost = dailyRate * totalDays;

            // Calculate extras cost
            var extrasCost = 0m;
            var extrasDetails = new List<ExtraCostDto>();
            
            if (request.ExtraIds.Any())
            {
                var extras = await _bookingRepository.GetExtraTypePricesByIdsAsync(request.ExtraIds);
                foreach (var extra in extras)
                {
                    var extraTotalCost = extra.DailyPrice * totalDays;
                    extrasCost += extraTotalCost;
                    
                    extrasDetails.Add(new ExtraCostDto
                    {
                        ExtraId = extra.Id,
                        ExtraName = extra.NameEn,
                        ExtraNameAr = extra.NameAr,
                        PricePerDay = extra.DailyPrice,
                        PricePerHour = extra.DailyPrice / 24,
                        Quantity = 1,
                        TotalCost = extraTotalCost,
                        PricingType = "PerDay"
                    });
                }
            }

            // Calculate delivery fee
            var deliveryFee = request.PickupBranchId != request.ReturnBranchId ? DELIVERY_FEE : 0m;

            // Calculate insurance fee
            var insuranceFee = baseCost * INSURANCE_PERCENTAGE / 100;

            // Calculate subtotal before discounts
            var subtotal = baseCost + extrasCost + deliveryFee + insuranceFee;

            // Apply discounts
            var discounts = new List<DiscountDto>();
            var totalDiscountAmount = 0m;

            // Long-term rental discount
            if (totalDays >= 7)
            {
                var discountPercentage = totalDays >= 30 ? 15m : 10m;
                var discountAmount = subtotal * discountPercentage / 100;
                totalDiscountAmount += discountAmount;
                
                discounts.Add(new DiscountDto
                {
                    DiscountType = "LongTermDiscount",
                    DiscountName = $"Long Term Rental Discount ({discountPercentage}%)",
                    DiscountAmount = discountAmount,
                    DiscountPercentage = discountPercentage,
                    Description = $"Discount for {totalDays} days rental"
                });
            }

            // TODO: Apply promo code discount if provided
            if (!string.IsNullOrEmpty(request.PromoCode))
            {
                // Placeholder for promo code validation and discount application
                // This would typically involve checking against a PromoCode entity
            }

            // Apply loyalty points discount
            var loyaltyPointsDiscount = 0m;
            var loyaltyPointsToRedeem = 0;
            
            if (request.LoyaltyPointsToRedeem.HasValue && 
                request.LoyaltyPointsToRedeem > 0 && 
                !string.IsNullOrEmpty(request.UserId))
            {
                var loyaltyResult = await _loyaltyPointService.GetUserLoyaltyPointSummaryAsync(request.UserId);
                if (loyaltyResult.Succeeded)
                {
                    var availablePoints = loyaltyResult.Data.AvailableLoyaltyPoints;
                    var maxRedeemableValue = subtotal * MAX_LOYALTY_REDEMPTION_PERCENTAGE / 100;
                    var maxRedeemablePoints = (int)(maxRedeemableValue / LOYALTY_POINT_VALUE);
                    
                    loyaltyPointsToRedeem = Math.Min(
                        Math.Min(request.LoyaltyPointsToRedeem.Value, availablePoints),
                        maxRedeemablePoints
                    );
                    
                    loyaltyPointsDiscount = loyaltyPointsToRedeem * LOYALTY_POINT_VALUE;
                }
            }

            // Calculate total after discounts
            var totalAfterDiscounts = subtotal - totalDiscountAmount - loyaltyPointsDiscount;

            // Calculate tax on final amount
            var taxAmount = totalAfterDiscounts * TAX_PERCENTAGE / 100;
            var finalAmount = totalAfterDiscounts + taxAmount;

            // Calculate loyalty points to earn
            var loyaltyPointsToEarn = (int)(finalAmount * LOYALTY_POINTS_PER_SAR);

            // Build comprehensive response
            var response = new BookingCostCalculationResponseDto
            {
                CarId = request.CarId,
                CarName = $"{car.BrandEn} {car.ModelEn}",
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalDays = totalDays,
                TotalHours = totalHours,

                BaseRatePerDay = dailyRate,
                BaseRatePerHour = dailyRate / 24,
                BaseCost = baseCost,

                Extras = extrasDetails,
                TotalExtrasCost = extrasCost,

                DeliveryFee = deliveryFee,
                ReturnFee = 0, // No separate return fee
                InsuranceFee = insuranceFee,
                TaxAmount = taxAmount,
                TaxPercentage = TAX_PERCENTAGE,

                Discounts = discounts,
                TotalDiscountAmount = totalDiscountAmount,

                LoyaltyPointsToRedeem = loyaltyPointsToRedeem,
                LoyaltyPointsDiscount = loyaltyPointsDiscount,
                LoyaltyPointsToEarn = loyaltyPointsToEarn,

                SubTotal = subtotal,
                TotalCost = totalAfterDiscounts,
                FinalAmount = finalAmount,

                Currency = "SAR",
                IsAvailable = true,
                CalculatedAt = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                success = false, 
                message = "An error occurred while calculating booking cost",
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Validate booking cost calculation request
    /// </summary>
    private (bool IsValid, string ErrorMessage) ValidateRequest(BookingCostCalculationRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return (false, "Invalid request data");
        }

        if (request.StartDate >= request.EndDate)
        {
            return (false, "End date must be after start date");
        }

        if (request.StartDate < DateTime.UtcNow.Date)
        {
            return (false, "Start date cannot be in the past");
        }

        var rentalPeriod = request.EndDate - request.StartDate;
        if (rentalPeriod.TotalDays > 365)
        {
            return (false, "Rental period cannot exceed 365 days");
        }

        if (request.CarId <= 0)
        {
            return (false, "Invalid car ID");
        }

        if (request.PickupBranchId <= 0 || request.ReturnBranchId <= 0)
        {
            return (false, "Invalid branch ID(s)");
        }

        if (request.LoyaltyPointsToRedeem.HasValue && request.LoyaltyPointsToRedeem < 0)
        {
            return (false, "Loyalty points to redeem cannot be negative");
        }

        return (true, string.Empty);
    }
}
