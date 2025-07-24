using System.ComponentModel.DataAnnotations;

namespace NOL.Application.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int CarId { get; set; }
    public string CarBrand { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
}

public class CreateReviewDto
{
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 1000 characters")]
    public string Comment { get; set; } = string.Empty;

    [Required]
    public int CarId { get; set; }
}

public class UpdateReviewDto
{
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 1000 characters")]
    public string Comment { get; set; } = string.Empty;
}

public class CarRatingDto
{
    public int CarId { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
}
