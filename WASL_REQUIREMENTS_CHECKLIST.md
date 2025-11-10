# WASL Integration Requirements Checklist

## 📋 Standard WASL Requirements for Rental Companies

Based on Saudi Arabian regulations for fleet management and the WASL system.

### ✅ **Mandatory Requirements**

#### 1. **Vehicle Registration**
- [ ] All rental vehicles must be registered in WASL
- [ ] Valid plate number (Arabic format)
- [ ] Sequence number for multi-digit plates
- [ ] VIN/Chassis number
- [ ] Vehicle category/type
- [ ] Owner company information

#### 2. **Driver Registration**
- [ ] National ID or Iqama number
- [ ] Full name (Arabic required)
- [ ] Valid Saudi driving license
- [ ] License expiry date
- [ ] Mobile number
- [ ] Date of birth

#### 3. **Location Tracking**
- [ ] Real-time GPS updates (minimum every 30 seconds for moving vehicles)
- [ ] Latitude and longitude (WGS84 format)
- [ ] Speed in km/h
- [ ] Heading/direction
- [ ] Timestamp (UTC)
- [ ] Engine status (on/off)

#### 4. **Rental Contracts**
- [ ] Contract registration when car is rented
- [ ] Customer information
- [ ] Rental start date/time
- [ ] Expected return date/time
- [ ] Driver assignment
- [ ] Contract closure when car is returned

### 📊 **Data Flow for Rental Companies**

```
┌─────────────────────────────────────────┐
│  1. Register Vehicles in WASL           │
│     - One-time per vehicle              │
│     - Store WASL Vehicle ID             │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  2. Register Drivers                    │
│     - One-time per driver               │
│     - Verify license validity           │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  3. Create Rental Contract              │
│     - When customer rents a car         │
│     - Assign driver to vehicle          │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  4. Send Location Updates               │
│     - Every 30 seconds (active rental)  │
│     - Include speed, heading, time      │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  5. Close Contract                      │
│     - When car is returned              │
│     - Update vehicle status             │
└─────────────────────────────────────────┘
```

### 🔑 **Required API Endpoints**

| Priority | Endpoint | Purpose | Status |
|----------|----------|---------|--------|
| HIGH | `POST /api/v2/vehicles/register` | Register vehicle | ✅ Implemented |
| HIGH | `POST /api/v2/location/update` | Send GPS location | ✅ Implemented |
| HIGH | `POST /api/v2/contracts/create` | Create rental contract | ⚠️ TODO |
| HIGH | `POST /api/v2/drivers/register` | Register driver | ✅ Implemented |
| MEDIUM | `POST /api/v2/contracts/close` | Close contract | ⚠️ TODO |
| MEDIUM | `PUT /api/v2/vehicles/{id}` | Update vehicle | ✅ Implemented |
| MEDIUM | `POST /api/v2/drivers/assign` | Assign driver | ✅ Implemented |
| LOW | `GET /api/v2/vehicles/{id}` | Get vehicle info | ✅ Implemented |

### ⚡ **Critical Integration Points**

#### When Customer Books a Car:
```csharp
1. Create booking in your database
2. Assign driver (customer) to vehicle in WASL
3. Create rental contract in WASL
4. Start sending location updates
```

#### During Active Rental:
```csharp
1. Send GPS location every 30-60 seconds
2. Monitor for violations/alerts
3. Track mileage and usage
```

#### When Car is Returned:
```csharp
1. Stop sending location updates
2. Close rental contract in WASL
3. Unassign driver from vehicle
4. Update booking status in your database
```

### 📝 **Typical WASL Request Format**

#### Vehicle Registration:
```json
{
  "companyId": "12345",
  "plateNumber": "أ ب ج 1234",
  "sequenceNumber": "1",
  "vehicleType": "1",
  "plateType": "3",
  "chassisNumber": "1HGCM82633A123456",
  "referenceKey": "YOUR-INTERNAL-CAR-ID"
}
```

#### Location Update:
```json
{
  "vehicleId": "WASL-VEH-ID",
  "latitude": 24.7136,
  "longitude": 46.6753,
  "speed": 65.5,
  "heading": 180,
  "timestamp": "2024-11-10T22:15:00Z",
  "engineStatus": true
}
```

#### Rental Contract:
```json
{
  "contractNumber": "RENT-2024-001",
  "vehicleId": "WASL-VEH-ID",
  "driverId": "WASL-DRV-ID",
  "startDate": "2024-11-10T10:00:00Z",
  "expectedEndDate": "2024-11-15T10:00:00Z",
  "customerId": "1234567890",
  "customerName": "أحمد محمد",
  "customerPhone": "+966501234567"
}
```

### 🎯 **What's Missing (Based on Standard WASL)**

From my current implementation, these are likely needed:

1. ⚠️ **Rental Contract Management**
   - Create contract endpoint
   - Close contract endpoint
   - Contract modification endpoint

2. ⚠️ **Location Update Scheduler**
   - Hangfire job for active rentals
   - GPS device integration
   - Batch updates for multiple cars

3. ⚠️ **Violation Alerts**
   - Speed violations
   - Geo-fence violations
   - Unauthorized driver alerts

4. ⚠️ **Trip Management**
   - Trip start/end
   - Trip summary
   - Odometer readings

### 💡 **Recommendations**

Please help me understand from the PDF:

1. What is the exact **authentication method**? (Bearer token/API Key/Other?)
2. What are the **actual endpoint URLs**? (Are they `/api/v2/...` or different?)
3. Is there a **rental contract** management requirement?
4. What is the **location update frequency** requirement?
5. Are there any **specific error codes** to handle?
6. What **additional fields** are required for vehicles?

Would you like me to:
- **A)** Create the rental contract management endpoints
- **B)** Add a location tracking Hangfire job
- **C)** Enhance the current implementation with specific details from the PDF
- **D)** All of the above

Please share the key sections from the PDF, or let me know which approach you'd prefer! 📄
