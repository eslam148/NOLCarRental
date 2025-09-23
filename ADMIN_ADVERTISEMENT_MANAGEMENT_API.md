## Admin Advertisement Management API - Frontend Documentation

Requires admin authentication with roles: `Admin`, `SuperAdmin`, or `BranchManager`.

Base route: `/api/admin/advertisements`

All responses use `ApiResponse<T>` with fields: `succeeded`, `message`, `errors`, `data`, `statusCode`, `statusCodeValue`.

### Common Types

- AdvertisementType: `Special | Discount | Seasonal | Flash | Weekend | Holiday | NewArrival | Popular`
- AdvertisementStatus: `Draft | Active | Paused | Expired | Canceled`
- PaginatedResponseDto<T>:
```json
{
  "data": [/* T */],
  "currentPage": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### DTOs

- AdminAdvertisementDto (extends `AdvertisementDto`):
```json
{
  "id": 1,
  "title": "Summer Sale",
  "description": "Up to 30% off",
  "price": 100.0,
  "discountPrice": 80.0,
  "discountPercentage": 20.0,
  "startDate": "2025-06-01T00:00:00Z",
  "endDate": "2025-06-30T23:59:59Z",
  "imageUrl": "https://...",
  "type": "Discount",
  "status": "Active",
  "viewCount": 1200,
  "clickCount": 240,
  "isFeatured": true,
  "sortOrder": 1,
  "createdAt": "2025-05-20T10:00:00Z",
  "updatedAt": "2025-05-25T10:00:00Z",
  "createdByAdminName": "Admin User",
  "clickThroughRate": 20.0,
  "conversionCount": 0,
  "conversionRate": 0.0,
  "revenueGenerated": 0.0,
  "performanceScore": 75.5,
  "dailyMetrics": []
}
```

- AdminCreateAdvertisementDto (request):

Required and constraints:
- titleAr: required, max 200 chars
- titleEn: required, max 200 chars
- descriptionAr: required, max 1000 chars
- descriptionEn: required, max 1000 chars
- type: required, enum AdvertisementType
- startDate: required, ISO 8601 datetime
- endDate: required, ISO 8601 datetime
- price: number, 0–10000 (default 0)
- imageUrl: URL (optional)
- discountPercentage: number, 0–100 (optional)
- discountPrice: number, 0–10000 (optional)
- isFeatured: boolean (default false)
- sortOrder: number, 0–1000 (default 0)
- isActive: boolean (default true)
- carId: number (optional)
- categoryId: number (optional)
- notes: max 500 chars (optional)

Example:
```json
{
  "titleAr": "عنوان",
  "titleEn": "Title",
  "descriptionAr": "وصف",
  "descriptionEn": "Description",
  "type": "Discount",
  "startDate": "2025-06-01T00:00:00Z",
  "endDate": "2025-06-30T23:59:59Z",
  "price": 100.0,
  "imageUrl": "https://example.com/banner.jpg",
  "discountPercentage": 20.0,
  "discountPrice": 80.0,
  "isFeatured": true,
  "sortOrder": 1,
  "isActive": true,
  "carId": 123,
  "categoryId": 45,
  "notes": "Optional notes"
}
```

- AdminUpdateAdvertisementDto (request): all fields optional
```json
{
  "titleAr": "عنوان محدث",
  "titleEn": "Updated Title",
  "descriptionAr": "وصف محدث",
  "descriptionEn": "Updated Description",
  "type": "Seasonal",
  "startDate": "2025-07-01T00:00:00Z",
  "endDate": "2025-07-31T23:59:59Z",
  "imageUrl": "https://...",
  "discountPercentage": 25.0,
  "discountPrice": 75.0,
  "isFeatured": false,
  "sortOrder": 2,
  "isActive": true,
  "status": "Paused",
  "carId": 456,
  "categoryId": 78,
  "notes": "Updated notes"
}
```

- AdvertisementFilterDto (query):
```
type, status, isActive, isFeatured, carId, categoryId, createdByAdminId,
startDateFrom, startDateTo, endDateFrom, endDateTo, createdDateFrom, createdDateTo,
minDiscountPercentage, maxDiscountPercentage, page, pageSize, sortBy, sortOrder, searchTerm
```

---

### Endpoints

All endpoints require header: `Authorization: Bearer {admin_jwt_token}`.

#### GET /api/admin/advertisements
Query: `AdvertisementFilterDto`
Response: `ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>`

#### GET /api/admin/advertisements/{id}
Response: `ApiResponse<AdminAdvertisementDto>`

#### POST /api/admin/advertisements
Body: `AdminCreateAdvertisementDto`
Response: `ApiResponse<AdminAdvertisementDto>`

#### PUT /api/admin/advertisements/{id}
Body: `AdminUpdateAdvertisementDto`
Response: `ApiResponse<AdminAdvertisementDto>`

#### DELETE /api/admin/advertisements/{id}
Response: `ApiResponse`

#### PATCH /api/admin/advertisements/{id}/status
Body: `AdvertisementStatus`
Response: `ApiResponse<AdminAdvertisementDto>`

#### PATCH /api/admin/advertisements/bulk/status?status={status}
Body: `[number]` (IDs)
Response: `ApiResponse`

#### PATCH /api/admin/advertisements/{id}/schedule?startDate=...&endDate=...
Response: `ApiResponse<AdminAdvertisementDto>`

#### POST /api/admin/advertisements/bulk/schedule
Body: `AdvertisementScheduleDto`
Response: `ApiResponse`

#### GET /api/admin/advertisements/scheduled?date=&page=&pageSize=
Response: `ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>`

#### GET /api/admin/advertisements/expired?page=&pageSize=
Response: `ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>`

#### PATCH /api/admin/advertisements/{id}/featured
Body: `boolean`
Response: `ApiResponse<AdminAdvertisementDto>`

#### GET /api/admin/advertisements/featured?page=&pageSize=
Response: `ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>`

#### PATCH /api/admin/advertisements/{id}/sort-order
Body: `number`
Response: `ApiResponse`

#### PATCH /api/admin/advertisements/{id}/discount?discountPercentage=&discountPrice=
Response: `ApiResponse<AdminAdvertisementDto>`

#### GET /api/admin/advertisements/with-discounts?page=&pageSize=
Response: `ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>`

#### GET /api/admin/advertisements/{id}/calculate-discount?originalPrice=
Response: `ApiResponse<number>`

#### GET /api/admin/advertisements/analytics?startDate=&endDate=
Response: `ApiResponse<AdvertisementAnalyticsDto>`

#### GET /api/admin/advertisements/analytics/top-performing?count=&startDate=&endDate=
Response: `ApiResponse<[AdvertisementPerformanceDto]>`

#### GET /api/admin/advertisements/analytics/low-performing?count=&startDate=&endDate=
Response: `ApiResponse<[AdvertisementPerformanceDto]>`

#### GET /api/admin/advertisements/{id}/metrics?startDate=&endDate=
Response: `ApiResponse<[AdvertisementMetricDto]>`

#### POST /api/admin/advertisements/{id}/view?userId=&ipAddress=
Response: `ApiResponse`

#### POST /api/admin/advertisements/{id}/click?userId=&ipAddress=
Response: `ApiResponse`

#### POST /api/admin/advertisements/{id}/conversion?userId=&conversionValue=
Response: `ApiResponse`

#### GET /api/admin/advertisements/analytics/type-stats?startDate=&endDate=
Response: `ApiResponse<[AdvertisementTypeStatsDto]>`

#### GET /api/admin/advertisements/by-type/{type}?activeOnly=&page=&pageSize=
Response: `ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>`

#### POST /api/admin/advertisements/bulk/operation
Body: `BulkAdvertisementOperationDto`
Response: `ApiResponse`

#### POST /api/admin/advertisements/copy
Body: `CopyAdvertisementDto`
Response: `ApiResponse<AdminAdvertisementDto>`

#### GET /api/admin/advertisements/templates?page=&pageSize=
Response: `ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>`

#### GET /api/admin/advertisements/by-car/{carId}?page=&pageSize=
Response: `ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>`

#### GET /api/admin/advertisements/by-category/{categoryId}?page=&pageSize=
Response: `ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>`

#### GET /api/admin/advertisements/{id}/revenue-impact?startDate=&endDate=
Response: `ApiResponse<number>`

#### GET /api/admin/advertisements/analytics/monthly/{year}
Response: `ApiResponse<[MonthlyAdvertisementStatsDto]>`

#### GET /api/admin/advertisements/revenue/total?startDate=&endDate=
Response: `ApiResponse<number>`

#### POST /api/admin/advertisements/report
Body: `AdvertisementFilterDto`
Response: `ApiResponse<AdvertisementReportDto>`

#### POST /api/admin/advertisements/export?format=excel|csv
Body: `AdvertisementFilterDto`
Response: File (binary)

#### GET /api/admin/advertisements/active?date=&page=&pageSize=
Response: `ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>`

#### GET /api/admin/advertisements/search?searchTerm=&page=&pageSize=
Response: `ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>`

### Headers
- Authorization: `Bearer {token}`
- Content-Type: `application/json` (for POST/PUT/PATCH with body)

### Example (fetch)
```javascript
async function getAdvertisements(token, params) {
  const qs = new URLSearchParams(params).toString();
  const res = await fetch(`/api/admin/advertisements?${qs}`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  const json = await res.json();
  if (!json.succeeded) throw new Error(json.message);
  return json.data; // PaginatedResponseDto
}
```

