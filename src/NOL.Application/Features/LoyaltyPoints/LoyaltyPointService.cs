using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;
using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Application.Features.LoyaltyPoints;

public class LoyaltyPointService : ILoyaltyPointService
{
    private readonly ILoyaltyPointRepository _loyaltyPointRepository;
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;

    // Business rules constants
    private const int POINTS_PER_DOLLAR = 1; // 1 point per dollar spent
    private const int POINTS_EXPIRY_MONTHS = 24; // Points expire after 2 years
    private const decimal POINT_VALUE = 0.01m; // Each point worth $0.01
    private const int MIN_REDEMPTION_POINTS = 100; // Minimum 100 points to redeem
    private const int REGISTRATION_BONUS_POINTS = 100;
    private const int REVIEW_POINTS = 50;
    private const int REFERRAL_POINTS = 500;

    public LoyaltyPointService(
        ILoyaltyPointRepository loyaltyPointRepository,
        LocalizedApiResponseService responseService,
        ILocalizationService localizationService)
    {
        _loyaltyPointRepository = loyaltyPointRepository;
        _responseService = responseService;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<LoyaltyPointSummaryDto>> GetUserLoyaltyPointSummaryAsync(string userId)
    {
        try
        {
            var availablePoints = await _loyaltyPointRepository.GetUserAvailablePointsAsync(userId);
            var totalPoints = await _loyaltyPointRepository.GetUserTotalPointsAsync(userId);
            var recentTransactions = await _loyaltyPointRepository.GetUserTransactionsAsync(userId, 1, 5);
            
            // Get points expiring in 30 days
            var expiringPoints = await _loyaltyPointRepository.GetExpiringPointsAsync(userId, DateTime.UtcNow.AddDays(30));
            var pointsExpiringIn30Days = expiringPoints.Sum(p => p.Points);

            // Calculate lifetime stats
            var allTransactions = await _loyaltyPointRepository.GetUserTransactionsAsync(userId, 1, int.MaxValue);
            var lifetimeEarned = allTransactions
                .Where(t => t.TransactionType == LoyaltyPointTransactionType.Earned ||
                           t.TransactionType == LoyaltyPointTransactionType.Bonus)
                .Sum(t => t.Points);

            var lifetimeRedeemed = allTransactions
                .Where(t => t.TransactionType == LoyaltyPointTransactionType.Redeemed)
                .Sum(t => t.Points);

            var summary = new LoyaltyPointSummaryDto
            {
                TotalLoyaltyPoints = totalPoints,
                AvailableLoyaltyPoints = availablePoints,
                LifetimePointsEarned = lifetimeEarned,
                LifetimePointsRedeemed = lifetimeRedeemed,
                PointsExpiringIn30Days = pointsExpiringIn30Days,
                RecentTransactions = recentTransactions.Select(MapToTransactionDto).ToList()
            };

            return _responseService.Success(summary, "LoyaltyPointSummaryRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<LoyaltyPointSummaryDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<LoyaltyPointTransactionDto>>> GetUserTransactionsAsync(string userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var transactions = await _loyaltyPointRepository.GetUserTransactionsAsync(userId, page, pageSize);
            var transactionDtos = transactions.Select(MapToTransactionDto).ToList();
            
            return _responseService.Success(transactionDtos, "LoyaltyPointTransactionsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<LoyaltyPointTransactionDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<LoyaltyPointTransactionDto>> AwardPointsAsync(AwardPointsDto awardDto)
    {
        try
        {
            var expiryDate = awardDto.ExpiryDate ?? DateTime.UtcNow.AddMonths(POINTS_EXPIRY_MONTHS);
            
            var transaction = new LoyaltyPointTransaction
            {
                UserId = awardDto.UserId,
                Points = awardDto.Points,
                TransactionType = LoyaltyPointTransactionType.Earned,
                EarnReason = awardDto.EarnReason,
                Description = awardDto.Description ?? GetEarnReasonDescription(awardDto.EarnReason),
                BookingId = awardDto.BookingId,
                ExpiryDate = expiryDate,
                TransactionDate = DateTime.UtcNow
            };

            var createdTransaction = await _loyaltyPointRepository.CreateTransactionAsync(transaction);
            
            // Update user's loyalty point totals
            await UpdateUserLoyaltyTotalsAsync(awardDto.UserId);

            var transactionDto = MapToTransactionDto(createdTransaction);
            return _responseService.Success(transactionDto, "LoyaltyPointsAwarded");
        }
        catch (Exception)
        {
            return _responseService.Error<LoyaltyPointTransactionDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<LoyaltyPointTransactionDto>> RedeemPointsAsync(string userId, RedeemPointsDto redeemDto)
    {
        try
        {
            if (redeemDto.PointsToRedeem < MIN_REDEMPTION_POINTS)
            {
                return _responseService.Error<LoyaltyPointTransactionDto>("MinimumRedemptionNotMet");
            }

            var availablePoints = await _loyaltyPointRepository.GetUserAvailablePointsAsync(userId);
            if (availablePoints < redeemDto.PointsToRedeem)
            {
                return _responseService.Error<LoyaltyPointTransactionDto>("InsufficientLoyaltyPoints");
            }

            var transaction = new LoyaltyPointTransaction
            {
                UserId = userId,
                Points = -redeemDto.PointsToRedeem, // Negative for redemption
                TransactionType = LoyaltyPointTransactionType.Redeemed,
                Description = redeemDto.Description ?? "Points redeemed for discount",
                BookingId = redeemDto.BookingId,
                TransactionDate = DateTime.UtcNow
            };

            var createdTransaction = await _loyaltyPointRepository.CreateTransactionAsync(transaction);
            
            // Update user's loyalty point totals
            await UpdateUserLoyaltyTotalsAsync(userId);

            var transactionDto = MapToTransactionDto(createdTransaction);
            return _responseService.Success(transactionDto, "LoyaltyPointsRedeemed");
        }
        catch (Exception)
        {
            return _responseService.Error<LoyaltyPointTransactionDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> ProcessBookingPointsAsync(string userId, int bookingId, decimal bookingAmount)
    {
        try
        {
            // Check if points already awarded for this booking
            var alreadyAwarded = await _loyaltyPointRepository.HasUserEarnedPointsForBookingAsync(userId, bookingId);
            if (alreadyAwarded)
            {
                return _responseService.Success(true, "PointsAlreadyAwarded");
            }

            var pointsToAward = CalculatePointsForAmount(bookingAmount);
            if (pointsToAward > 0)
            {
                var awardDto = new AwardPointsDto
                {
                    UserId = userId,
                    Points = pointsToAward,
                    EarnReason = LoyaltyPointEarnReason.BookingCompleted,
                    BookingId = bookingId,
                    Description = $"Points earned for booking completion - ${bookingAmount:F2}"
                };

                await AwardPointsAsync(awardDto);
            }

            return _responseService.Success(true, "BookingPointsProcessed");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> ExpireOldPointsAsync()
    {
        try
        {
            // This would typically be called by a background job
            var allUsers = await _loyaltyPointRepository.GetUserTransactionsAsync("", 1, int.MaxValue);
            var userIds = allUsers.Select(t => t.UserId).Distinct();

            foreach (var userId in userIds)
            {
                var expiringPoints = await _loyaltyPointRepository.GetExpiringPointsAsync(userId, DateTime.UtcNow);
                if (expiringPoints.Any())
                {
                    var transactionIds = expiringPoints.Select(p => p.Id).ToList();
                    await _loyaltyPointRepository.ExpirePointsAsync(transactionIds);
                    
                    // Update user totals
                    await UpdateUserLoyaltyTotalsAsync(userId);
                }
            }

            return _responseService.Success(true, "ExpiredPointsProcessed");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    public int CalculatePointsForAmount(decimal amount)
    {
        return (int)Math.Floor(amount * POINTS_PER_DOLLAR);
    }

    public decimal CalculateDiscountForPoints(int points)
    {
        return points * POINT_VALUE;
    }

    private async Task UpdateUserLoyaltyTotalsAsync(string userId)
    {
        var availablePoints = await _loyaltyPointRepository.GetUserAvailablePointsAsync(userId);
        var totalPoints = await _loyaltyPointRepository.GetUserTotalPointsAsync(userId);
        
        // Calculate lifetime stats from all transactions
        var allTransactions = await _loyaltyPointRepository.GetUserTransactionsAsync(userId, 1, int.MaxValue);
        
        var lifetimeEarned = allTransactions
            .Where(t => t.TransactionType == LoyaltyPointTransactionType.Earned ||
                       t.TransactionType == LoyaltyPointTransactionType.Bonus)
            .Sum(t => t.Points);

        var lifetimeRedeemed = allTransactions
            .Where(t => t.TransactionType == LoyaltyPointTransactionType.Redeemed)
            .Sum(t => Math.Abs(t.Points));

        await _loyaltyPointRepository.UpdateUserLoyaltyPointsAsync(userId, availablePoints, totalPoints, lifetimeEarned, lifetimeRedeemed);
    }

    private LoyaltyPointTransactionDto MapToTransactionDto(LoyaltyPointTransaction transaction)
    {
        return new LoyaltyPointTransactionDto
        {
            Id = transaction.Id,
            Points = transaction.Points,
            TransactionType = transaction.TransactionType,
            EarnReason = transaction.EarnReason,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            ExpiryDate = transaction.ExpiryDate,
            IsExpired = transaction.IsExpired,
            BookingId = transaction.BookingId,
            BookingNumber = transaction.Booking?.BookingNumber
        };
    }

    private string GetEarnReasonDescription(LoyaltyPointEarnReason earnReason)
    {
        return earnReason switch
        {
            LoyaltyPointEarnReason.BookingCompleted => "Points earned for completed booking",
            LoyaltyPointEarnReason.Registration => "Welcome bonus points",
            LoyaltyPointEarnReason.Referral => "Referral bonus points",
            LoyaltyPointEarnReason.Review => "Points for writing a review",
            LoyaltyPointEarnReason.Birthday => "Birthday bonus points",
            LoyaltyPointEarnReason.Promotion => "Promotional bonus points",
            LoyaltyPointEarnReason.LongTermRental => "Long-term rental bonus",
            LoyaltyPointEarnReason.PremiumCar => "Premium car bonus points",
            _ => "Loyalty points earned"
        };
    }
} 