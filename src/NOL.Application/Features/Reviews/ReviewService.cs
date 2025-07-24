using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;
using NOL.Domain.Entities;

namespace NOL.Application.Features.Reviews;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICarRepository _carRepository;
    private readonly ILoyaltyPointService _loyaltyPointService;
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;

    public ReviewService(
        IReviewRepository reviewRepository,
        IBookingRepository bookingRepository,
        ICarRepository carRepository,
        ILoyaltyPointService loyaltyPointService,
        LocalizedApiResponseService responseService,
        ILocalizationService localizationService)
    {
        _reviewRepository = reviewRepository;
        _bookingRepository = bookingRepository;
        _carRepository = carRepository;
        _loyaltyPointService = loyaltyPointService;
        _responseService = responseService;
        _localizationService = localizationService;
    }

    // Simple mobile rating - optimized for mobile usage
    public async Task<ApiResponse<SimpleMobileRatingResponseDto>> RateCarSimpleAsync(string userId, int carId, SimpleMobileRatingDto ratingDto)
    {
        try
        {
            // 1. Verify user has rented this car
            var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
            var hasCompletedBooking = bookings.Any(b =>
                b.CarId == carId &&
                b.Status == Domain.Enums.BookingStatus.Completed);

            if (!hasCompletedBooking)
            {
                var errorResponse = new SimpleMobileRatingResponseDto
                {
                    Success = false,
                    Message = "You must rent this car before you can rate it.",
                    PointsAwarded = 0,
                    NewCarAverageRating = 0,
                    TotalCarReviews = 0
                };

                return new ApiResponse<SimpleMobileRatingResponseDto>
                {
                    Succeeded = false,
                    Message = "You must rent this car before you can rate it.",
                    Data = errorResponse,
                    StatusCode = Domain.Enums.ApiStatusCode.BadRequest
                };
            }

            // 2. Check if user already rated this car
            var existingReview = await _reviewRepository.GetReviewByUserAndCarAsync(userId, carId);
            if (existingReview != null)
            {
                // Update existing review instead of creating new one
                existingReview.Rating = ratingDto.Rating;

                await _reviewRepository.UpdateAsync(existingReview);
                await _reviewRepository.SaveChangesAsync();

                // Get updated car statistics
                var updatedStats = await GetCarRatingStatsAsync(carId);
                
                return _responseService.Success(new SimpleMobileRatingResponseDto
                {
                    Success = true,
                    Message = "Rating updated successfully",
                    PointsAwarded = 0, // No points for updates
                    NewCarAverageRating = updatedStats.averageRating,
                    TotalCarReviews = updatedStats.totalReviews
                }, "RatingUpdated");
            }

            // 3. Verify car exists
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                var notFoundResponse = new SimpleMobileRatingResponseDto
                {
                    Success = false,
                    Message = "Car not found.",
                    PointsAwarded = 0,
                    NewCarAverageRating = 0,
                    TotalCarReviews = 0
                };

                return new ApiResponse<SimpleMobileRatingResponseDto>
                {
                    Succeeded = false,
                    Message = "Car not found.",
                    Data = notFoundResponse,
                    StatusCode = Domain.Enums.ApiStatusCode.NotFound
                };
            }

            // 4. Create simple review with auto-generated comment
            var review = new Review
            {
                Rating = ratingDto.Rating,
                Comment = GenerateSimpleComment(ratingDto.Rating),
                CarId = carId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveChangesAsync();

            // 5. Award loyalty points
            const int pointsForRating = 10;
            var awardDto = new AwardPointsDto
            {
                UserId = userId,
                Points = pointsForRating,
                Description = "Car rating",
                EarnReason = Domain.Enums.LoyaltyPointEarnReason.Review
            };
            await _loyaltyPointService.AwardPointsAsync(awardDto);

            // 6. Get updated car statistics
            var carStats = await GetCarRatingStatsAsync(carId);

            // 7. Return simple response
            return _responseService.Success(new SimpleMobileRatingResponseDto
            {
                Success = true,
                Message = "Thank you for rating this car!",
                PointsAwarded = pointsForRating,
                NewCarAverageRating = carStats.averageRating,
                TotalCarReviews = carStats.totalReviews
            }, "CarRatedSuccessfully");
        }
        catch (Exception)
        {
            var errorResponse = new SimpleMobileRatingResponseDto
            {
                Success = false,
                Message = "An error occurred while processing your rating. Please try again.",
                PointsAwarded = 0,
                NewCarAverageRating = 0,
                TotalCarReviews = 0
            };

            return new ApiResponse<SimpleMobileRatingResponseDto>
            {
                Succeeded = false,
                Message = "An error occurred while processing your rating. Please try again.",
                Data = errorResponse,
                StatusCode = Domain.Enums.ApiStatusCode.InternalServerError
            };
        }
    }

    private string GenerateSimpleComment(int rating)
    {
        return rating switch
        {
            5 => "Excellent car! Highly recommended.",
            4 => "Great experience with this car.",
            3 => "Good car overall.",
            2 => "Average experience.",
            1 => "Not satisfied with this car.",
            _ => "Quick rating."
        };
    }

    private async Task<(decimal averageRating, int totalReviews)> GetCarRatingStatsAsync(int carId)
    {
        var reviews = await _reviewRepository.GetReviewsByCarIdAsync(carId);
        var averageRating = reviews.Any() ? Math.Round((decimal)reviews.Average(r => r.Rating), 1) : 0;
        var totalReviews = reviews.Count();
        
        return (averageRating, totalReviews);
    }

    // Basic implementations for interface compliance
    public async Task<ApiResponse<List<ReviewDto>>> GetReviewsByCarIdAsync(int carId)
    {
        try
        {
            var reviews = await _reviewRepository.GetReviewsByCarIdAsync(carId);
            var reviewDtos = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UserName = r.User.FullName,
                UserId = r.UserId,
                CarId = r.CarId,
                CarBrand = r.Car.BrandEn,
                CarModel = r.Car.ModelEn
            }).ToList();

            return _responseService.Success(reviewDtos, "ReviewsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<ReviewDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<ReviewDto>>> GetReviewsByUserIdAsync(string userId)
    {
        try
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            var reviewDtos = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UserName = r.User.FullName,
                UserId = r.UserId,
                CarId = r.CarId,
                CarBrand = r.Car.BrandEn,
                CarModel = r.Car.ModelEn
            }).ToList();

            return _responseService.Success(reviewDtos, "UserReviewsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<ReviewDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<ReviewDto>> GetReviewByIdAsync(int id)
    {
        try
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                return _responseService.NotFound<ReviewDto>("ReviewNotFound");
            }

            var reviewDto = new ReviewDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UserName = review.User.FullName,
                UserId = review.UserId,
                CarId = review.CarId,
                CarBrand = review.Car.BrandEn,
                CarModel = review.Car.ModelEn
            };

            return _responseService.Success(reviewDto, "ReviewRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<ReviewDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<CarRatingDto>> GetCarRatingAsync(int carId)
    {
        try
        {
            var reviews = await _reviewRepository.GetReviewsByCarIdAsync(carId);
            var reviewDtos = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UserName = r.User.FullName,
                UserId = r.UserId,
                CarId = r.CarId,
                CarBrand = r.Car.BrandEn,
                CarModel = r.Car.ModelEn
            }).ToList();

            var averageRating = reviews.Any() ? Math.Round((decimal)reviews.Average(r => r.Rating), 1) : 0;

            var carRatingDto = new CarRatingDto
            {
                CarId = carId,
                AverageRating = averageRating,
                TotalReviews = reviews.Count(),
                Reviews = reviewDtos
            };

            return _responseService.Success(carRatingDto, "CarRatingRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<CarRatingDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<ReviewDto>> CreateReviewAsync(string userId, CreateReviewDto createReviewDto)
    {
        // Basic implementation - can be enhanced later
        return _responseService.Error<ReviewDto>("NotImplemented");
    }

    public async Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int id, string userId, UpdateReviewDto updateReviewDto)
    {
        // Basic implementation - can be enhanced later
        return _responseService.Error<ReviewDto>("NotImplemented");
    }

    public async Task<ApiResponse<bool>> DeleteReviewAsync(int id, string userId)
    {
        // Basic implementation - can be enhanced later
        return _responseService.Error<bool>("NotImplemented");
    }

    public async Task<ApiResponse<bool>> CanUserReviewCarAsync(string userId, int carId)
    {
        try
        {
            // Check if user has completed a booking for this car
            var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
            var hasCompletedBooking = bookings.Any(b =>
                b.CarId == carId &&
                b.Status == Domain.Enums.BookingStatus.Completed);

            return _responseService.Success(hasCompletedBooking, hasCompletedBooking ? "CanReview" : "MustCompleteBookingToReview");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }
}
