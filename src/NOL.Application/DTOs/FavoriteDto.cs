namespace NOL.Application.DTOs;

public class FavoriteDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CarId { get; set; }
    public DateTime CreatedAt { get; set; }
    public CarDto Car { get; set; } = null!;
}

public class AddFavoriteDto
{
    public int CarId { get; set; }
} 