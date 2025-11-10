# WASL API Integration Guide

## 📖 Overview

WASL (وصل) is the **Saudi Arabian government's fleet management and tracking system** developed by Elm Company. All commercial vehicles in Saudi Arabia must be registered and tracked through WASL.

**Base URL:** `https://wasl.api.elm.sa`

## 🎯 Features Implemented

✅ Vehicle Registration
✅ Vehicle Information Updates
✅ Location Tracking (GPS)
✅ Batch Location Updates
✅ Driver Registration
✅ Driver-Vehicle Assignment
✅ Trip History Tracking
✅ Health Check

## 🏗️ Architecture

```
┌─────────────────────────────────────────┐
│  Controllers (Your API)                 │
│  - WaslTestController (Testing)         │
│  - Admin Controllers (Production)       │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  IWaslService (Wrapper Service)         │
│  - High-level business logic            │
│  - Error handling & logging             │
│  - Data mapping                         │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  IWaslApiService (Refit Interface)      │
│  - HTTP client abstraction              │
│  - Automatic serialization              │
│  - Retry & timeout handling             │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  WASL API (https://wasl.api.elm.sa)    │
│  - Saudi Government System              │
└─────────────────────────────────────────┘
```

## ⚙️ Configuration

### 1. Update `appsettings.json`:

```json
{
  "ExternalApis": {
    "WASL": {
      "BaseUrl": "https://wasl.api.elm.sa",
      "ApiKey": "YOUR_WASL_API_KEY_HERE",
      "CompanyId": "YOUR_COMPANY_ID_HERE",
      "Enabled": false
    }
  }
}
```

### 2. Enable WASL Integration:

```json
{
  "ExternalApis": {
    "WASL": {
      "Enabled": true  // Change to true when you have credentials
    }
  }
}
```

### 3. Set Environment Variables (Production):

```bash
# For production, use environment variables instead of config files
export WASL__ApiKey="your-actual-api-key"
export WASL__CompanyId="your-company-id"
```

## 📋 Required Data in Car Entity

The Car entity now includes WASL-specific fields:

```csharp
public class Car
{
    // ... existing fields ...
    
    public string PlateNumber { get; set; }           // Required
    public string? SequenceNumber { get; set; }       // Optional (plate sequence)
    public string? ChassisNumber { get; set; }        // Optional (VIN)
    public string? WaslVehicleId { get; set; }        // WASL system ID (set after registration)
}
```

## 🚀 Usage Examples

### Example 1: Register a Car in WASL

```csharp
public class CarService
{
    private readonly IWaslService _waslService;
    
    public async Task RegisterCarInWasl(Car car)
    {
        var companyId = "NOL-RENTAL-001";
        var result = await _waslService.RegisterCarAsync(car, companyId);
        
        if (result.Success)
        {
            car.WaslVehicleId = result.Data.WaslVehicleId;
            await _carRepository.UpdateAsync(car);
        }
    }
}
```

### Example 2: Send Location Update

```csharp
// Send real-time GPS location of a rented car
await _waslService.SendCarLocationAsync(
    waslVehicleId: car.WaslVehicleId,
    latitude: 24.7136,
    longitude: 46.6753,
    speed: 80.5  // km/h
);
```

### Example 3: Check API Health

```csharp
var isHealthy = await _waslService.IsWaslApiHealthyAsync();
if (!isHealthy)
{
    _logger.LogWarning("WASL API is not available");
}
```

## 🧪 Testing Endpoints

### 1. Check WASL Configuration

```http
GET /api/wasltest/config
```

**Response:**
```json
{
  "enabled": false,
  "baseUrl": "https://wasl.api.elm.sa",
  "hasValidApiKey": false,
  "message": "WASL integration is disabled. Enable it in appsettings.json",
  "endpoints": {
    "health": "/api/wasltest/health",
    "registerCar": "/api/wasltest/register-car/{carId}",
    "sendLocation": "/api/wasltest/send-location/{carId}",
    "getVehicleInfo": "/api/wasltest/vehicle-info/{carId}",
    "registerDriver": "/api/wasltest/register-driver"
  }
}
```

### 2. Check WASL API Health

```http
GET /api/wasltest/health
```

**Response:**
```json
{
  "success": true,
  "waslApiHealthy": true,
  "message": "WASL API is healthy",
  "baseUrl": "https://wasl.api.elm.sa",
  "timestamp": "2024-11-10T22:10:00Z"
}
```

### 3. Register a Car

```http
POST /api/wasltest/register-car/1
Authorization: Bearer {admin_token}
```

**Response:**
```json
{
  "success": true,
  "message": "Vehicle registered successfully",
  "waslVehicleId": "WASL-12345678",
  "carId": 1,
  "plateNumber": "ABC-1234",
  "timestamp": "2024-11-10T22:10:00Z"
}
```

### 4. Send Location Update

```http
POST /api/wasltest/send-location/1
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "latitude": 24.7136,
  "longitude": 46.6753,
  "speed": 80.5
}
```

**Response:**
```json
{
  "success": true,
  "message": "Location updated successfully",
  "carId": 1,
  "waslVehicleId": "WASL-12345678",
  "location": {
    "latitude": 24.7136,
    "longitude": 46.6753,
    "speed": 80.5
  },
  "timestamp": "2024-11-10T22:10:00Z"
}
```

### 5. Get Vehicle Info from WASL

```http
GET /api/wasltest/vehicle-info/1
Authorization: Bearer {admin_token}
```

## 📡 Available API Endpoints

### Vehicle Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/vehicles/register` | Register new vehicle |
| PUT | `/vehicles/{vehicleId}` | Update vehicle info |
| DELETE | `/vehicles/{vehicleId}` | Unregister vehicle |
| GET | `/vehicles/{vehicleId}` | Get vehicle information |
| GET | `/vehicles` | List all vehicles |

### Location Tracking

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/locations` | Send single location update |
| POST | `/locations/batch` | Send batch location updates |

### Driver Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/drivers/register` | Register new driver |
| PUT | `/drivers/{driverId}` | Update driver info |
| GET | `/drivers/{driverId}` | Get driver information |
| POST | `/vehicles/{vehicleId}/assign-driver` | Assign driver to vehicle |

### Trip Tracking

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/vehicles/{vehicleId}/trips` | Get vehicle trip history |

## 🔑 Authentication

All WASL API requests require Bearer token authentication:

```
Authorization: Bearer YOUR_API_KEY
```

The API key is configured in `appsettings.json` and automatically added by the service layer.

## 📊 Response Format

All WASL API responses follow this standard format:

```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... },
  "errors": [],
  "errorCode": null,
  "timestamp": "2024-11-10T22:10:00Z"
}
```

## 🔄 Integration Workflow

### Registering a New Car

```
1. Add car to your database (NOL system)
   ↓
2. Call RegisterCarAsync() to register in WASL
   ↓
3. WASL returns WaslVehicleId
   ↓
4. Save WaslVehicleId in your Car entity
   ↓
5. Car is now tracked in both systems
```

### Tracking Active Rentals

```
1. Customer rents a car (booking created)
   ↓
2. Car status changes to "Rented"
   ↓
3. GPS tracker sends location updates
   ↓
4. Your system forwards locations to WASL
   ↓
5. WASL tracks vehicle movement in real-time
   ↓
6. Booking ends, car returned
   ↓
7. Stop sending location updates
```

## 🧪 Testing Locally

### Step 1: Check Configuration

```bash
curl http://localhost:5000/api/wasltest/config
```

### Step 2: Test Health Check

```bash
curl http://localhost:5000/api/wasltest/health
```

### Step 3: Register a Test Car

```bash
curl -X POST http://localhost:5000/api/wasltest/register-car/1 \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

### Step 4: Send Test Location

```bash
curl -X POST http://localhost:5000/api/wasltest/send-location/1 \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "latitude": 24.7136,
    "longitude": 46.6753,
    "speed": 65.0
  }'
```

## ⚠️ Important Notes

### 1. API Credentials Required

To use WASL API, you need:
- ✅ **API Key** from Elm Company
- ✅ **Company ID** registered with WASL
- ✅ **Commercial license** for fleet management

### 2. Rate Limiting

WASL API has rate limits:
- Location updates: Max 1 per minute per vehicle
- Registration operations: Max 100 per hour
- Query operations: Max 1000 per hour

### 3. Data Requirements

**For Vehicle Registration:**
- Valid Saudi Arabia plate number
- VIN/Chassis number (recommended)
- Vehicle type and plate type
- Company ID

**For Location Updates:**
- Valid GPS coordinates
- Timestamp (UTC)
- Vehicle must be registered first

### 4. Production Deployment

**Before going to production:**

1. Get actual WASL credentials
2. Store API key in environment variables (NOT in appsettings.json)
3. Enable WASL integration: `"Enabled": true`
4. Test with sandbox environment first (if available)
5. Implement location tracking scheduler

## 🔐 Security Best Practices

### ❌ Don't Do This:
```json
{
  "WASL": {
    "ApiKey": "actual-api-key-in-plain-text"
  }
}
```

### ✅ Do This Instead:

**Option 1: Environment Variables**
```bash
export ExternalApis__WASL__ApiKey="your-key"
export ExternalApis__WASL__CompanyId="your-company-id"
```

**Option 2: Azure Key Vault**
```csharp
builder.Configuration.AddAzureKeyVault(...);
```

**Option 3: User Secrets (Development)**
```bash
dotnet user-secrets set "ExternalApis:WASL:ApiKey" "your-dev-key"
```

## 📈 Advanced Usage

### Batch Location Updates (Efficient)

```csharp
var locations = new List<LocationUpdateRequest>
{
    new() { VehicleId = "WASL-123", Latitude = 24.7, Longitude = 46.6, ... },
    new() { VehicleId = "WASL-456", Latitude = 24.8, Longitude = 46.7, ... },
    // ... up to 100 vehicles at once
};

var request = new BatchLocationRequest { Locations = locations };
var result = await _waslApiService.SendBatchLocationsAsync(request, authorization);
```

### Automatic Location Tracking (Hangfire Job)

```csharp
// Create a new Hangfire job to send location updates
public class WaslLocationTrackingJob
{
    public async Task Execute()
    {
        // Get all cars currently rented
        var rentedCars = await _carRepository.GetRentedCarsAsync();
        
        foreach (var car in rentedCars)
        {
            if (!string.IsNullOrEmpty(car.WaslVehicleId))
            {
                // Get GPS location from your tracking device
                var location = await _gpsService.GetCarLocationAsync(car.Id);
                
                // Send to WASL
                await _waslService.SendCarLocationAsync(
                    car.WaslVehicleId,
                    location.Latitude,
                    location.Longitude,
                    location.Speed);
            }
        }
    }
}

// Schedule in Program.cs
recurringJobs.AddOrUpdate<WaslLocationTrackingJob>(
    "wasl-location-tracking",
    job => job.Execute(),
    "*/5 * * * *"  // Every 5 minutes
);
```

## 📚 API Reference

### Vehicle Types (WaslVehicleType)

| Value | Arabic | English |
|-------|--------|---------|
| 1 | سيارة خاصة | Private Car |
| 2 | شاحنة | Truck |
| 3 | دراجة نارية | Motorcycle |
| 4 | حافلة | Bus |
| 5 | مركبة تجارية | Commercial Vehicle |

### Plate Types (WaslPlateType)

| Value | Arabic | English |
|-------|--------|---------|
| 1 | لوحة خاصة | Private |
| 2 | نقل عام | Public Transport |
| 3 | تجاري | Commercial |
| 4 | حكومي | Government |
| 5 | دبلوماسي | Diplomatic |

## 🐛 Troubleshooting

### Common Issues

#### 1. "Unauthorized" Error
```
Problem: Invalid or missing API key
Solution: Check API key in configuration
```

#### 2. "Vehicle already registered"
```
Problem: Trying to register same plate number twice
Solution: Use update endpoint instead, or delete first
```

#### 3. "Invalid coordinates"
```
Problem: Lat/Lng out of valid range
Solution: Ensure coordinates are in Saudi Arabia (lat: 16-32, lng: 34-56)
```

#### 4. "Rate limit exceeded"
```
Problem: Too many requests
Solution: Implement rate limiting in your application
```

## 📞 Support

### WASL Support Contacts:
- **Website:** https://www.elm.sa
- **Support:** support@elm.sa
- **Phone:** +966 11 218 XXXX

### Documentation:
- Official API Docs: https://wasl.api.elm.sa/docs
- Developer Portal: https://developer.elm.sa

## ✅ Checklist for Production

- [ ] Obtain WASL API credentials from Elm Company
- [ ] Register your company in WASL system
- [ ] Store API key in environment variables (NOT in config files)
- [ ] Add all vehicles with valid plate numbers
- [ ] Test in sandbox environment (if available)
- [ ] Implement GPS tracking integration
- [ ] Set up location update scheduler
- [ ] Configure error notifications
- [ ] Enable WASL integration in production config
- [ ] Monitor API usage and rate limits
- [ ] Set up alerting for failed API calls

## 🎯 Next Steps

1. **Get WASL Credentials:**
   - Contact Elm Company
   - Register your company
   - Get API key and company ID

2. **Add Migration for New Fields:**
   ```bash
   dotnet ef migrations add AddWaslFieldsToCar -p src/NOL.Infrastructure -s src/NOL.API
   ```

3. **Update Car Management:**
   - Add plate number validation
   - Add chassis number input fields
   - Auto-register cars in WASL when added

4. **Implement GPS Tracking:**
   - Integrate with GPS device provider
   - Create location tracking service
   - Schedule regular updates to WASL

5. **Monitor & Maintain:**
   - Set up health checks
   - Monitor API usage
   - Handle WASL system downtime gracefully

## 📄 Sample Data

### Sample Vehicle Registration Request:
```json
{
  "plateNumber": "ABC-1234",
  "sequenceNumber": "5678",
  "vehicleType": 1,
  "vehicleId": "1HGCM82633A123456",
  "plateType": 3,
  "companyId": "NOL-RENTAL-001",
  "referenceKey": "CAR-001",
  "additionalInfo": {
    "brand": "Toyota",
    "model": "Camry",
    "year": 2024,
    "color": "White"
  }
}
```

### Sample Location Update:
```json
{
  "vehicleId": "WASL-12345678",
  "latitude": 24.7136,
  "longitude": 46.6753,
  "speed": 65.0,
  "heading": 180.5,
  "gpsTimestamp": "2024-11-10T22:10:00Z",
  "odometer": 45680.5,
  "engineStatus": true
}
```

---

**Created with ❤️ for NOL Car Rental**

For questions or issues, contact your development team.

