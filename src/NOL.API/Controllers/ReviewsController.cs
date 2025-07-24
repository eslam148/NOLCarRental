using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.DTOs;
using System.Security.Claims;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Simple mobile car rating endpoint - POST /api/reviews/rate/{carId}
    /// Accepts only car ID and rating (1-5 stars) for optimal mobile experience
    /// </summary>
    /// <param name="carId">ID of the car to rate</param>
    /// <param name="ratingDto">Simple rating object with just the star rating</param>
    /// <returns>Simple success/error response with loyalty points and updated car stats</returns>
    [HttpPost("rate/{carId}")]
    [Authorize]
    public async Task<IActionResult> RateCarSimple(int carId, [FromBody] SimpleMobileRatingDto ratingDto)
    {
        // Validate input
        if (!ModelState.IsValid)
        {
            return BadRequest(new SimpleMobileRatingResponseDto
            {
                Success = false,
                Message = "Invalid rating. Please provide a rating between 1 and 5 stars.",
                PointsAwarded = 0,
                NewCarAverageRating = 0,
                TotalCarReviews = 0
            });
        }

        // Get authenticated user ID from JWT token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new SimpleMobileRatingResponseDto
            {
                Success = false,
                Message = "User authentication required to rate cars.",
                PointsAwarded = 0,
                NewCarAverageRating = 0,
                TotalCarReviews = 0
            });
        }

        // Validate car ID
        if (carId <= 0)
        {
            return BadRequest(new SimpleMobileRatingResponseDto
            {
                Success = false,
                Message = "Invalid car ID provided.",
                PointsAwarded = 0,
                NewCarAverageRating = 0,
                TotalCarReviews = 0
            });
        }

        // Process the rating
        var result = await _reviewService.RateCarSimpleAsync(userId, carId, ratingDto);
        
        // Return appropriate HTTP status with the response
        return StatusCode(result.StatusCodeValue, result.Data);
    }

    /// <summary>
    /// Get car rating summary
    /// </summary>
    [HttpGet("car/{carId}/rating")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCarRating(int carId)
    {
        var result = await _reviewService.GetCarRatingAsync(carId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get reviews for a specific car
    /// </summary>
    [HttpGet("car/{carId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviewsByCarId(int carId)
    {
        var result = await _reviewService.GetReviewsByCarIdAsync(carId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Check if user can review a specific car
    /// </summary>
    [HttpGet("can-review/{carId}")]
    [Authorize]
    public async Task<IActionResult> CanReviewCar(int carId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var result = await _reviewService.CanUserReviewCarAsync(userId, carId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get user's own reviews
    /// </summary>
    [HttpGet("my-reviews")]
    [Authorize]
    public async Task<IActionResult> GetMyReviews()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var result = await _reviewService.GetReviewsByUserIdAsync(userId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get a specific review by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviewById(int id)
    {
        var result = await _reviewService.GetReviewByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }
}
