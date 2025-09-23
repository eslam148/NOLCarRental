## Extras API - Frontend Documentation

### Overview
Public endpoints to retrieve available extras and their pricing. Responses follow the common `ApiResponse<T>` shape and are localized based on the active language middleware (Arabic/English). No authentication is required for these endpoints.

### Data Model

#### ExtraDto
```json
{
  "id": 1,
  "extraType": "GPS",
  "name": "GPS Navigation",
  "description": "Turn-by-turn navigation device",
  "dailyPrice": 10.00,
  "weeklyPrice": 60.00,
  "monthlyPrice": 200.00
}
```

Notes:
- `name` and `description` are localized by server (Arabic when culture is `ar`, English when `en`).
- `extraType` is an enum string; see values below.

#### ApiResponse<T>
All endpoints return the following wrapper:
```json
{
  "succeeded": true,
  "message": "ExtrasRetrieved",
  "internalMessage": null,
  "errors": [],
  "stackTrace": null,
  "data": [/* T or T[] here */],
  "statusCode": "Success",
  "statusCodeValue": 200
}
```

### Enum: ExtraType
Supported values:
- GPS
- ChildSeat
- AdditionalDriver
- Insurance
- WifiHotspot
- PhoneCharger
- Bluetooth
- RoofRack
- SkiRack
- BikeRack

---

### Endpoints

#### GET /api/extras
Get all extras (active and inactive may be included depending on data; prefer `/api/extras/active` for only active ones).

Response (`ApiResponse<List<ExtraDto>>`):
```json
{
  "succeeded": true,
  "message": "ExtrasRetrieved",
  "data": [
    {
      "id": 1,
      "extraType": "GPS",
      "name": "GPS Navigation",
      "description": "Turn-by-turn navigation device",
      "dailyPrice": 10.0,
      "weeklyPrice": 60.0,
      "monthlyPrice": 200.0
    }
  ],
  "errors": [],
  "statusCode": "Success",
  "statusCodeValue": 200
}
```

#### GET /api/extras/{id}
Get a single extra by ID.

Path params:
- `id` (number): Extra identifier

Response (`ApiResponse<ExtraDto>`):
```json
{
  "succeeded": true,
  "message": "ExtraRetrieved",
  "data": {
    "id": 1,
    "extraType": "GPS",
    "name": "GPS Navigation",
    "description": "Turn-by-turn navigation device",
    "dailyPrice": 10.0,
    "weeklyPrice": 60.0,
    "monthlyPrice": 200.0
  },
  "errors": [],
  "statusCode": "Success",
  "statusCodeValue": 200
}
```

On not found:
```json
{
  "succeeded": false,
  "message": "ResourceNotFound",
  "data": null,
  "errors": [],
  "statusCode": "NotFound",
  "statusCodeValue": 404
}
```

#### GET /api/extras/active
Get only active extras.

Response (`ApiResponse<List<ExtraDto>>`): same shape as `GET /api/extras`.

#### GET /api/extras/type/{type}
Get extras filtered by `ExtraType`.

Path params:
- `type` (string): One of the enum values listed above (case-insensitive by ASP.NET model binding if sent as number or exact enum string).

Response (`ApiResponse<List<ExtraDto>>`): same shape as `GET /api/extras`.

---

### Localization
- The API auto-detects language via middleware. You may send `Accept-Language: en` or `Accept-Language: ar` header to control `name`/`description` language.

### Frontend Usage Examples

JavaScript (fetch):
```javascript
// Get active extras
async function getActiveExtras() {
  const response = await fetch('/api/extras/active', {
    headers: { 'Accept-Language': 'en' }
  });
  const result = await response.json();
  if (result.succeeded) return result.data;
  throw new Error(result.message || 'Failed to load extras');
}

// Get extras by type
async function getExtrasByType(type) {
  const response = await fetch(`/api/extras/type/${type}`, {
    headers: { 'Accept-Language': 'en' }
  });
  const result = await response.json();
  if (result.succeeded) return result.data;
  throw new Error(result.message || 'Failed to load extras');
}
```

TypeScript interface hint:
```ts
export type ExtraType =
  | 'GPS' | 'ChildSeat' | 'AdditionalDriver' | 'Insurance'
  | 'WifiHotspot' | 'PhoneCharger' | 'Bluetooth' | 'RoofRack'
  | 'SkiRack' | 'BikeRack';

export interface ExtraDto {
  id: number;
  extraType: ExtraType;
  name: string;
  description: string;
  dailyPrice: number;
  weeklyPrice: number;
  monthlyPrice: number;
}
```

### Notes
- All endpoints are public; no auth headers required.
- Use `/api/extras/active` for customer-facing lists.
- Prices are decimals; render with appropriate currency.

