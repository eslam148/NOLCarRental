# Paginated Favorites API - Implementation Guide

## Overview

The Favorites API has been enhanced with pagination support to efficiently handle large lists of user favorites. This implementation provides both paginated and non-paginated endpoints for maximum flexibility.

## 🎯 Key Features

### ✅ **Pagination Support**
- **Default pagination**: 10 items per page
- **Configurable page size**: 1-100 items per page
- **Complete pagination metadata**: Total count, pages, navigation info
- **Optimized database queries**: Skip/Take for efficient data retrieval

### ✅ **Dual Endpoint Strategy**
- **Paginated endpoint**: `GET /api/favorites` (default)
- **All items endpoint**: `GET /api/favorites/all` (for backward compatibility)

## 🚀 API Endpoints

### **GET /api/favorites** (Paginated - Default)

**Description**: Get user's favorite cars with pagination

**Authentication**: Required (JWT Bearer token)

**Query Parameters**:
- `page` (optional): Page number (default: 1, minimum: 1)
- `pageSize` (optional): Items per page (default: 10, minimum: 1, maximum: 100)

**Request Example**:
```http
GET /api/favorites?page=2&pageSize=5
Authorization: Bearer <jwt-token>
```

**Response**:
```json
{
  "succeeded": true,
  "message": "FavoritesRetrieved",
  "data": {
    "favorites": [
      {
        "id": 1,
        "userId": "user123",
        "carId": 5,
        "createdAt": "2024-07-20T10:30:00Z",
        "car": {
          "id": 5,
          "brandEn": "Toyota",
          "brandAr": "تويوتا",
          "modelEn": "Camry",
          "modelAr": "كامري",
          "year": 2023,
          "dailyRate": 150.00,
          "imageUrl": "https://example.com/car5.jpg",
          "category": {
            "id": 2,
            "nameEn": "Sedan",
            "nameAr": "سيدان"
          },
          "branch": {
            "id": 1,
            "nameEn": "Riyadh Main",
            "nameAr": "الرياض الرئيسي"
          },
          "rateCount": 15,
          "averageRating": 4.5,
          "isFavorite": true
        }
      }
    ],
    "currentPage": 2,
    "pageSize": 5,
    "totalCount": 23,
    "totalPages": 5,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "statusCode": 200
}
```

### **GET /api/favorites/all** (Non-Paginated)

**Description**: Get all user's favorite cars (no pagination)

**Authentication**: Required (JWT Bearer token)

**Request Example**:
```http
GET /api/favorites/all
Authorization: Bearer <jwt-token>
```

**Response**:
```json
{
  "succeeded": true,
  "message": "FavoritesRetrieved",
  "data": [
    {
      "id": 1,
      "userId": "user123",
      "carId": 5,
      "createdAt": "2024-07-20T10:30:00Z",
      "car": {
        "id": 5,
        "brandEn": "Toyota",
        "brandAr": "تويوتا",
        "modelEn": "Camry",
        "modelAr": "كامري",
        "year": 2023,
        "dailyRate": 150.00,
        "imageUrl": "https://example.com/car5.jpg",
        "category": {
          "id": 2,
          "nameEn": "Sedan",
          "nameAr": "سيدان"
        },
        "branch": {
          "id": 1,
          "nameEn": "Riyadh Main",
          "nameAr": "الرياض الرئيسي"
        },
        "rateCount": 15,
        "averageRating": 4.5,
        "isFavorite": true
      }
    }
  ],
  "statusCode": 200
}
```

## 🔧 Implementation Details

### **Database Layer**

#### **Repository Interface** (`IFavoriteRepository`)
```csharp
public interface IFavoriteRepository : IRepository<Favorite>
{
    Task<IEnumerable<Favorite>> GetUserFavoritesAsync(string userId);
    Task<IEnumerable<Favorite>> GetUserFavoritesPagedAsync(string userId, int page, int pageSize);
    Task<int> GetUserFavoritesCountAsync(string userId);
    Task<Favorite?> GetFavoriteAsync(string userId, int carId);
    Task<bool> IsFavoriteAsync(string userId, int carId);
    Task RemoveFavoriteAsync(string userId, int carId);
}
```

#### **Repository Implementation** (`FavoriteRepository`)
```csharp
public async Task<IEnumerable<Favorite>> GetUserFavoritesPagedAsync(string userId, int page, int pageSize)
{
    return await _dbSet
        .Include(f => f.Car)
            .ThenInclude(c => c.Category)
        .Include(f => f.Car)
            .ThenInclude(c => c.Branch)
        .Include(f => f.Car)
            .ThenInclude(c => c.Reviews)
        .Where(f => f.UserId == userId)
        .OrderByDescending(f => f.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
}

public async Task<int> GetUserFavoritesCountAsync(string userId)
{
    return await _dbSet
        .Where(f => f.UserId == userId)
        .CountAsync();
}
```

### **Service Layer**

#### **Service Interface** (`IFavoriteService`)
```csharp
public interface IFavoriteService
{
    Task<ApiResponse<List<FavoriteDto>>> GetUserFavoritesAsync(string userId);
    Task<ApiResponse<PaginatedFavoritesDto>> GetUserFavoritesPagedAsync(string userId, int page = 1, int pageSize = 10);
    Task<ApiResponse<FavoriteDto>> AddToFavoritesAsync(string userId, AddFavoriteDto addFavoriteDto);
    Task<ApiResponse<bool>> RemoveFromFavoritesAsync(string userId, int carId);
    Task<ApiResponse<bool>> IsFavoriteAsync(string userId, int carId);
}
```

#### **Service Implementation** (`FavoriteService`)
```csharp
public async Task<ApiResponse<PaginatedFavoritesDto>> GetUserFavoritesPagedAsync(string userId, int page = 1, int pageSize = 10)
{
    try
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Limit max page size

        // Get paginated favorites and total count
        var favorites = await _favoriteRepository.GetUserFavoritesPagedAsync(userId, page, pageSize);
        var totalCount = await _favoriteRepository.GetUserFavoritesCountAsync(userId);

        // Map to DTOs
        var favoriteDtos = favorites.Select(MapToFavoriteDto).ToList();

        // Calculate pagination info
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var hasPreviousPage = page > 1;
        var hasNextPage = page < totalPages;

        var paginatedResult = new PaginatedFavoritesDto
        {
            Favorites = favoriteDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = hasPreviousPage,
            HasNextPage = hasNextPage
        };

        return _responseService.Success(paginatedResult, "FavoritesRetrieved");
    }
    catch (Exception)
    {
        return _responseService.Error<PaginatedFavoritesDto>("InternalServerError");
    }
}
```

### **DTOs**

#### **PaginatedFavoritesDto**
```csharp
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
```

### **Controller Layer**

#### **FavoritesController**
```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<PaginatedFavoritesDto>>> GetMyFavorites(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized();
    }

    var result = await _favoriteService.GetUserFavoritesPagedAsync(userId, page, pageSize);
    return StatusCode(result.StatusCodeValue, result);
}

[HttpGet("all")]
public async Task<ActionResult<ApiResponse<List<FavoriteDto>>>> GetAllMyFavorites()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized();
    }

    var result = await _favoriteService.GetUserFavoritesAsync(userId);
    return StatusCode(result.StatusCodeValue, result);
}
```

## 📱 Frontend Integration Examples

### **React/JavaScript**
```javascript
// Paginated favorites
const getFavorites = async (page = 1, pageSize = 10) => {
  try {
    const response = await fetch(`/api/favorites?page=${page}&pageSize=${pageSize}`, {
      headers: {
        'Authorization': `Bearer ${authToken}`,
        'Content-Type': 'application/json'
      }
    });
    
    const result = await response.json();
    
    if (result.succeeded) {
      const { favorites, currentPage, totalPages, hasNextPage, hasPreviousPage } = result.data;
      
      // Update UI with favorites and pagination info
      displayFavorites(favorites);
      updatePaginationControls(currentPage, totalPages, hasNextPage, hasPreviousPage);
      
      return result.data;
    }
  } catch (error) {
    console.error('Error fetching favorites:', error);
  }
};

// All favorites (non-paginated)
const getAllFavorites = async () => {
  try {
    const response = await fetch('/api/favorites/all', {
      headers: {
        'Authorization': `Bearer ${authToken}`,
        'Content-Type': 'application/json'
      }
    });
    
    const result = await response.json();
    return result.succeeded ? result.data : [];
  } catch (error) {
    console.error('Error fetching all favorites:', error);
    return [];
  }
};

// Pagination component example
const FavoritesPagination = ({ currentPage, totalPages, onPageChange }) => {
  return (
    <div className="pagination">
      <button 
        disabled={currentPage === 1}
        onClick={() => onPageChange(currentPage - 1)}
      >
        Previous
      </button>
      
      <span>Page {currentPage} of {totalPages}</span>
      
      <button 
        disabled={currentPage === totalPages}
        onClick={() => onPageChange(currentPage + 1)}
      >
        Next
      </button>
    </div>
  );
};
```

### **Mobile App Integration (iOS Swift)**
```swift
struct PaginatedFavoritesResponse: Codable {
    let favorites: [FavoriteDto]
    let currentPage: Int
    let pageSize: Int
    let totalCount: Int
    let totalPages: Int
    let hasPreviousPage: Bool
    let hasNextPage: Bool
}

func getFavorites(page: Int = 1, pageSize: Int = 10) async throws -> PaginatedFavoritesResponse {
    let url = URL(string: "https://api.nolcarrental.com/api/favorites?page=\(page)&pageSize=\(pageSize)")!
    var request = URLRequest(url: url)
    request.setValue("Bearer \(authToken)", forHTTPHeaderField: "Authorization")
    
    let (data, _) = try await URLSession.shared.data(for: request)
    let response = try JSONDecoder().decode(ApiResponse<PaginatedFavoritesResponse>.self, from: data)
    
    guard response.succeeded, let data = response.data else {
        throw APIError.invalidResponse
    }
    
    return data
}
```

## 🔒 Security & Performance

### **Security Features**
- **JWT Authentication**: Required for all endpoints
- **User Isolation**: Users can only access their own favorites
- **Input Validation**: Page and pageSize parameters validated
- **Rate Limiting**: Configurable limits on API calls

### **Performance Optimizations**
- **Database Indexing**: Optimized queries with proper indexes
- **Eager Loading**: Includes related entities (Car, Category, Branch, Reviews)
- **Pagination Limits**: Maximum 100 items per page
- **Efficient Counting**: Separate count query for total items

### **Validation Rules**
- `page`: Minimum 1, defaults to 1
- `pageSize`: Minimum 1, maximum 100, defaults to 10
- Invalid parameters are automatically corrected

## ✅ Implementation Status

- [x] Paginated repository methods implemented
- [x] Service layer with pagination support
- [x] Controller endpoints (paginated and non-paginated)
- [x] DTOs for paginated responses
- [x] Input validation and parameter sanitization
- [x] Database optimization with proper includes
- [x] Backward compatibility maintained
- [x] Complete pagination metadata
- [x] Error handling and security
- [x] Documentation and examples

The **Paginated Favorites API** provides efficient, scalable access to user favorites with complete pagination support while maintaining backward compatibility! 🚀📄
