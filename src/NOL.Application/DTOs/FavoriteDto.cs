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

public class RemoveFavoriteDto
{
    public int CarId { get; set; }
}

// Paginated favorites response
public class PaginatedFavoritesDto
{
    public List<FavoriteDto> Favorites { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}