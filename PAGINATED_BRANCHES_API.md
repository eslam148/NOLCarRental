# Paginated Branches API - Implementation Guide

## Overview

The Branches API has been enhanced with pagination support to efficiently handle large lists of branch locations. This implementation provides both paginated and non-paginated endpoints for maximum flexibility.

## 🎯 Key Features

### ✅ **Pagination Support**
- **Default pagination**: 10 items per page
- **Configurable page size**: 1-100 items per page
- **Complete pagination metadata**: Total count, pages, navigation info
- **Optimized database queries**: Skip/Take for efficient data retrieval
- **Alphabetical sorting**: Branches sorted by English name

### ✅ **Dual Endpoint Strategy**
- **Paginated endpoint**: `GET /api/branches` (default)
- **All items endpoint**: `GET /api/branches/all` (for backward compatibility)

## 🚀 API Endpoints

### **GET /api/branches** (Paginated - Default)

**Description**: Get branch locations with pagination

**Authentication**: Not required (public endpoint)

**Query Parameters**:
- `page` (optional): Page number (default: 1, minimum: 1)
- `pageSize` (optional): Items per page (default: 10, minimum: 1, maximum: 100)

**Request Example**:
```http
GET /api/branches?page=2&pageSize=5
```

**Response**:
```json
{
  "succeeded": true,
  "message": "BranchesRetrieved",
  "data": {
    "branches": [
      {
        "id": 1,
        "name": "Riyadh Main Branch",
        "description": "Main branch in Riyadh city center",
        "address": "King Fahd Road, Al Olaya District",
        "city": "Riyadh",
        "country": "Saudi Arabia",
        "phone": "+966-11-123-4567",
        "email": "riyadh@nolcarrental.com",
        "latitude": 24.7136,
        "longitude": 46.6753,
        "workingHours": "Sunday-Thursday: 8:00 AM - 10:00 PM, Friday-Saturday: 2:00 PM - 10:00 PM"
      },
      {
        "id": 2,
        "name": "Jeddah Airport Branch",
        "description": "Branch located at King Abdulaziz International Airport",
        "address": "King Abdulaziz International Airport, Terminal 1",
        "city": "Jeddah",
        "country": "Saudi Arabia",
        "phone": "+966-12-987-6543",
        "email": "jeddah@nolcarrental.com",
        "latitude": 21.6796,
        "longitude": 39.1564,
        "workingHours": "Daily: 24 hours"
      }
    ],
    "currentPage": 2,
    "pageSize": 5,
    "totalCount": 15,
    "totalPages": 3,
    "hasPreviousPage": true,
    "hasNextPage": true
  },
  "statusCode": 200
}
```

### **GET /api/branches/all** (Non-Paginated)

**Description**: Get all branch locations (no pagination)

**Authentication**: Not required (public endpoint)

**Request Example**:
```http
GET /api/branches/all
```

**Response**:
```json
{
  "succeeded": true,
  "message": "BranchesRetrieved",
  "data": [
    {
      "id": 1,
      "name": "Riyadh Main Branch",
      "description": "Main branch in Riyadh city center",
      "address": "King Fahd Road, Al Olaya District",
      "city": "Riyadh",
      "country": "Saudi Arabia",
      "phone": "+966-11-123-4567",
      "email": "riyadh@nolcarrental.com",
      "latitude": 24.7136,
      "longitude": 46.6753,
      "workingHours": "Sunday-Thursday: 8:00 AM - 10:00 PM, Friday-Saturday: 2:00 PM - 10:00 PM"
    }
  ],
  "statusCode": 200
}
```

### **Other Existing Endpoints** (Non-Paginated)

#### **GET /api/branches/{id}**
Get specific branch by ID

#### **GET /api/branches/country/{country}**
Get branches by country (e.g., "Saudi Arabia")

#### **GET /api/branches/city/{city}**
Get branches by city (e.g., "Riyadh")

## 🔧 Implementation Details

### **Database Layer**

#### **Repository Interface** (`IBranchRepository`)
```csharp
public interface IBranchRepository : IRepository<Branch>
{
    Task<IEnumerable<Branch>> GetActiveBranchesAsync();
    Task<IEnumerable<Branch>> GetActiveBranchesPagedAsync(int page, int pageSize);
    Task<int> GetActiveBranchesCountAsync();
    Task<Branch?> GetActiveBranchByIdAsync(int id);
    Task<IEnumerable<Branch>> GetBranchesByCountryAsync(string country);
    Task<IEnumerable<Branch>> GetBranchesByCityAsync(string city);
}
```

#### **Repository Implementation** (`BranchRepository`)
```csharp
public async Task<IEnumerable<Branch>> GetActiveBranchesPagedAsync(int page, int pageSize)
{
    return await _dbSet
        .Where(b => b.IsActive)
        .OrderBy(b => b.NameEn)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
}

public async Task<int> GetActiveBranchesCountAsync()
{
    return await _dbSet
        .Where(b => b.IsActive)
        .CountAsync();
}
```

### **Service Layer**

#### **Service Interface** (`IBranchService`)
```csharp
public interface IBranchService
{
    Task<ApiResponse<List<BranchDto>>> GetBranchesAsync();
    Task<ApiResponse<PaginatedBranchesDto>> GetBranchesPagedAsync(int page = 1, int pageSize = 10);
    Task<ApiResponse<BranchDto>> GetBranchByIdAsync(int id);
    Task<ApiResponse<List<BranchDto>>> GetBranchesByCountryAsync(string country);
    Task<ApiResponse<List<BranchDto>>> GetBranchesByCityAsync(string city);
}
```

#### **Service Implementation** (`BranchService`)
```csharp
public async Task<ApiResponse<PaginatedBranchesDto>> GetBranchesPagedAsync(int page = 1, int pageSize = 10)
{
    try
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Limit max page size

        // Get paginated branches and total count
        var branches = await _branchRepository.GetActiveBranchesPagedAsync(page, pageSize);
        var totalCount = await _branchRepository.GetActiveBranchesCountAsync();

        // Map to DTOs
        var branchDtos = branches.Select(MapToBranchDto).ToList();

        // Calculate pagination info
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var hasPreviousPage = page > 1;
        var hasNextPage = page < totalPages;

        var paginatedResult = new PaginatedBranchesDto
        {
            Branches = branchDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = hasPreviousPage,
            HasNextPage = hasNextPage
        };

        return _responseService.Success(paginatedResult, "BranchesRetrieved");
    }
    catch (Exception)
    {
        return _responseService.Error<PaginatedBranchesDto>("InternalServerError");
    }
}
```

### **DTOs**

#### **PaginatedBranchesDto**
```csharp
public class PaginatedBranchesDto
{
    public List<BranchDto> Branches { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
```

### **Controller Layer**

#### **BranchesController**
```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<PaginatedBranchesDto>>> GetBranches(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    var result = await _branchService.GetBranchesPagedAsync(page, pageSize);
    return StatusCode(result.StatusCodeValue, result);
}

[HttpGet("all")]
public async Task<ActionResult<ApiResponse<List<BranchDto>>>> GetAllBranches()
{
    var result = await _branchService.GetBranchesAsync();
    return StatusCode(result.StatusCodeValue, result);
}
```

## 📱 Frontend Integration Examples

### **React/JavaScript**
```javascript
// Paginated branches
const getBranches = async (page = 1, pageSize = 10) => {
  try {
    const response = await fetch(`/api/branches?page=${page}&pageSize=${pageSize}`);
    const result = await response.json();
    
    if (result.succeeded) {
      const { branches, currentPage, totalPages, hasNextPage, hasPreviousPage } = result.data;
      
      // Update UI with branches and pagination info
      displayBranches(branches);
      updatePaginationControls(currentPage, totalPages, hasNextPage, hasPreviousPage);
      
      return result.data;
    }
  } catch (error) {
    console.error('Error fetching branches:', error);
  }
};

// All branches (non-paginated)
const getAllBranches = async () => {
  try {
    const response = await fetch('/api/branches/all');
    const result = await response.json();
    return result.succeeded ? result.data : [];
  } catch (error) {
    console.error('Error fetching all branches:', error);
    return [];
  }
};

// Branch selection component
const BranchSelector = ({ onBranchSelect }) => {
  const [branches, setBranches] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    const loadBranches = async () => {
      const data = await getBranches(currentPage, 5);
      if (data) {
        setBranches(data.branches);
        setTotalPages(data.totalPages);
      }
    };
    loadBranches();
  }, [currentPage]);

  return (
    <div className="branch-selector">
      <div className="branches-list">
        {branches.map(branch => (
          <div key={branch.id} className="branch-item" onClick={() => onBranchSelect(branch)}>
            <h3>{branch.name}</h3>
            <p>{branch.address}</p>
            <p>{branch.city}, {branch.country}</p>
          </div>
        ))}
      </div>
      
      <div className="pagination">
        <button 
          disabled={currentPage === 1}
          onClick={() => setCurrentPage(currentPage - 1)}
        >
          Previous
        </button>
        <span>Page {currentPage} of {totalPages}</span>
        <button 
          disabled={currentPage === totalPages}
          onClick={() => setCurrentPage(currentPage + 1)}
        >
          Next
        </button>
      </div>
    </div>
  );
};
```

### **Mobile App Integration (iOS Swift)**
```swift
struct PaginatedBranchesResponse: Codable {
    let branches: [BranchDto]
    let currentPage: Int
    let pageSize: Int
    let totalCount: Int
    let totalPages: Int
    let hasPreviousPage: Bool
    let hasNextPage: Bool
}

func getBranches(page: Int = 1, pageSize: Int = 10) async throws -> PaginatedBranchesResponse {
    let url = URL(string: "https://api.nolcarrental.com/api/branches?page=\(page)&pageSize=\(pageSize)")!
    let (data, _) = try await URLSession.shared.data(from: url)
    let response = try JSONDecoder().decode(ApiResponse<PaginatedBranchesResponse>.self, from: data)
    
    guard response.succeeded, let data = response.data else {
        throw APIError.invalidResponse
    }
    
    return data
}
```

## 🔒 Security & Performance

### **Security Features**
- **Public Access**: No authentication required for branch information
- **Input Validation**: Page and pageSize parameters validated
- **Active Branches Only**: Only returns active branches
- **Rate Limiting**: Configurable limits on API calls

### **Performance Optimizations**
- **Database Indexing**: Optimized queries with proper indexes on IsActive and NameEn
- **Pagination Limits**: Maximum 100 items per page
- **Efficient Counting**: Separate count query for total items
- **Alphabetical Sorting**: Consistent ordering by English name

### **Validation Rules**
- `page`: Minimum 1, defaults to 1
- `pageSize`: Minimum 1, maximum 100, defaults to 10
- Invalid parameters are automatically corrected
- Only active branches are returned

## ✅ Implementation Status

- [x] Paginated repository methods implemented
- [x] Service layer with pagination support
- [x] Controller endpoints (paginated and non-paginated)
- [x] DTOs for paginated responses
- [x] Input validation and parameter sanitization
- [x] Database optimization with proper sorting
- [x] Backward compatibility maintained
- [x] Complete pagination metadata
- [x] Error handling and performance optimization
- [x] Documentation and examples

## 🎯 Use Cases

### **Branch Selection for Booking**
- **Car Pickup**: Paginated list for selecting pickup location
- **Car Return**: Paginated list for selecting return location
- **Nearby Branches**: Filter by city/country with pagination

### **Branch Locator**
- **Map Integration**: Load branches in chunks for better performance
- **Search Results**: Paginated search results by location
- **Mobile Apps**: Efficient loading for mobile interfaces

The **Paginated Branches API** provides efficient, scalable access to branch locations with complete pagination support while maintaining backward compatibility! 🚀📍
