namespace NOL.Application.DTOs.Common;

/// <summary>
/// Generic paginated response DTO for all admin endpoints
/// </summary>
/// <typeparam name="T">The type of data being paginated</typeparam>
public class PaginatedResponseDto<T>
{
    public List<T> Data { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    
    /// <summary>
    /// Creates a paginated response
    /// </summary>
    public static PaginatedResponseDto<T> Create(
        List<T> data, 
        int currentPage, 
        int pageSize, 
        int totalCount)
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return new PaginatedResponseDto<T>
        {
            Data = data,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = currentPage > 1,
            HasNextPage = currentPage < totalPages
        };
    }
}

/// <summary>
/// Base pagination filter for all admin endpoints
/// </summary>
public class BasePaginationFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
    public string? SearchTerm { get; set; }
    
    /// <summary>
    /// Validates and normalizes pagination parameters
    /// </summary>
    public void ValidateAndNormalize()
    {
        if (Page < 1) Page = 1;
        if (PageSize < 1) PageSize = 10;
        if (PageSize > 100) PageSize = 100; // Limit max page size
        
        SortOrder = SortOrder?.ToLower() == "desc" ? "desc" : "asc";
    }
}
