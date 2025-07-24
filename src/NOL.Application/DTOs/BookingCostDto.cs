using System.ComponentModel.DataAnnotations;

namespace NOL.Application.DTOs;

public class BookingCostCalculationRequestDto
{
    [Required]
    public int CarId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public int PickupBranchId { get; set; }

    [Required]
    public int ReturnBranchId { get; set; }

    public List<int> ExtraIds { get; set; } = new();

    public string? PromoCode { get; set; }

    public int? LoyaltyPointsToRedeem { get; set; }

    public string? UserId { get; set; }
}

public class BookingCostCalculationResponseDto
{
    public int CarId { get; set; }
    public string CarName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public int TotalHours { get; set; }

    // Base costs
    public decimal BaseRatePerDay { get; set; }
    public decimal BaseRatePerHour { get; set; }
    public decimal BaseCost { get; set; }

    // Extra costs
    public List<ExtraCostDto> Extras { get; set; } = new();
    public decimal TotalExtrasCost { get; set; }

    // Fees and charges
    public decimal DeliveryFee { get; set; }
    public decimal ReturnFee { get; set; }
    public decimal InsuranceFee { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TaxPercentage { get; set; }

    // Discounts
    public List<DiscountDto> Discounts { get; set; } = new();
    public decimal TotalDiscountAmount { get; set; }

    // Loyalty points
    public int LoyaltyPointsToRedeem { get; set; }
    public decimal LoyaltyPointsDiscount { get; set; }
    public int LoyaltyPointsToEarn { get; set; }

    // Final totals
    public decimal SubTotal { get; set; }
    public decimal TotalCost { get; set; }
    public decimal FinalAmount { get; set; }

    // Additional info
    public string Currency { get; set; } = "SAR";
    public bool IsAvailable { get; set; }
    public string? UnavailabilityReason { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class ExtraCostDto
{
    public int ExtraId { get; set; }
    public string ExtraName { get; set; } = string.Empty;
    public string ExtraNameAr { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public decimal PricePerHour { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal TotalCost { get; set; }
    public string PricingType { get; set; } = string.Empty;
}

public class DiscountDto
{
    public string DiscountType { get; set; } = string.Empty;
    public string DiscountName { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class QuickCostEstimationRequestDto
{
    [Required]
    public int CarId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public List<int> ExtraIds { get; set; } = new();
}

public class QuickCostEstimationResponseDto
{
    public decimal EstimatedCost { get; set; }
    public int TotalDays { get; set; }
    public decimal DailyRate { get; set; }
    public decimal ExtrasTotal { get; set; }
    public string Currency { get; set; } = "SAR";
    public bool IsAvailable { get; set; }
    public string? Message { get; set; }
}
