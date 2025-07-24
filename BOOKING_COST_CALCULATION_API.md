# Booking Cost Calculation API - Complete Implementation

## Overview

This document describes the implementation of a single, comprehensive API endpoint for calculating booking costs in the NOL Car Rental system. The API provides complete cost breakdown including base rates, extras, fees, taxes, discounts, and loyalty points integration.

## 🎯 Key Features

### ✅ **Single Comprehensive Endpoint**
- **Endpoint**: `POST /api/booking/calculate-cost`
- **Complete cost breakdown** with all components
- **Car availability checking** integrated
- **Loyalty points calculation** included
- **Saudi Arabia tax compliance** (15% VAT)

### ✅ **Cost Components Calculated**
1. **Base Rental Cost**: Daily rate × number of days
2. **Extra Services**: GPS, child seats, additional equipment
3. **Delivery Fee**: 50 SAR if pickup/return branches differ
4. **Insurance Fee**: 5% of base cost
5. **Tax (VAT)**: 15% of final amount (Saudi Arabia standard)
6. **Discounts**: Long-term rental discounts (10% for 7+ days, 15% for 30+ days)
7. **Loyalty Points**: Redemption (up to 50% of booking) and earning calculation

## 🚀 API Endpoint

### **POST /api/booking/calculate-cost**

**Description**: Calculate comprehensive booking cost with detailed breakdown

**Authentication**: Optional (supports both authenticated and anonymous users)

**Request Body**:
```json
{
  "carId": 1,
  "startDate": "2024-07-25T10:00:00Z",
  "endDate": "2024-07-28T10:00:00Z",
  "pickupBranchId": 1,
  "returnBranchId": 2,
  "extraIds": [1, 2, 3],
  "promoCode": "SUMMER2024",
  "loyaltyPointsToRedeem": 100
}
```

**Response (Success)**:
```json
{
  "carId": 1,
  "carName": "Toyota Camry",
  "startDate": "2024-07-25T10:00:00Z",
  "endDate": "2024-07-28T10:00:00Z",
  "totalDays": 3,
  "totalHours": 72,
  
  "baseRatePerDay": 150.00,
  "baseRatePerHour": 6.25,
  "baseCost": 450.00,
  
  "extras": [
    {
      "extraId": 1,
      "extraName": "GPS Navigation",
      "extraNameAr": "نظام تحديد المواقع",
      "pricePerDay": 25.00,
      "pricePerHour": 1.04,
      "quantity": 1,
      "totalCost": 75.00,
      "pricingType": "PerDay"
    },
    {
      "extraId": 2,
      "extraName": "Child Safety Seat",
      "extraNameAr": "مقعد أمان للأطفال",
      "pricePerDay": 20.00,
      "pricePerHour": 0.83,
      "quantity": 1,
      "totalCost": 60.00,
      "pricingType": "PerDay"
    }
  ],
  "totalExtrasCost": 135.00,
  
  "deliveryFee": 50.00,
  "returnFee": 0.00,
  "insuranceFee": 22.50,
  "taxAmount": 98.63,
  "taxPercentage": 15.0,
  
  "discounts": [],
  "totalDiscountAmount": 0.00,
  
  "loyaltyPointsToRedeem": 100,
  "loyaltyPointsDiscount": 10.00,
  "loyaltyPointsToEarn": 657,
  
  "subTotal": 657.50,
  "totalCost": 647.50,
  "finalAmount": 657.50,
  
  "currency": "SAR",
  "isAvailable": true,
  "calculatedAt": "2024-07-24T15:30:00Z"
}
```

**Response (Car Unavailable)**:
```json
{
  "carId": 1,
  "carName": "Toyota Camry",
  "startDate": "2024-07-25T10:00:00Z",
  "endDate": "2024-07-28T10:00:00Z",
  "isAvailable": false,
  "unavailabilityReason": "Car is not available for the selected dates",
  "finalAmount": 0,
  "currency": "SAR"
}
```

**Response (Error)**:
```json
{
  "success": false,
  "message": "End date must be after start date",
  "errors": ["End date must be after start date"]
}
```

## 🔧 Implementation Details

### **Cost Calculation Logic**

1. **Input Validation**:
   ```csharp
   // Comprehensive validation
   - Start date cannot be in the past
   - End date must be after start date
   - Rental period cannot exceed 365 days
   - Car ID and branch IDs must be valid
   - Loyalty points cannot be negative
   ```

2. **Base Cost Calculation**:
   ```csharp
   var totalDays = (int)Math.Ceiling((endDate - startDate).TotalDays);
   var baseCost = car.DailyRate * totalDays;
   ```

3. **Extras Calculation**:
   ```csharp
   var extrasCost = extras.Sum(e => e.DailyPrice * totalDays);
   ```

4. **Fees Calculation**:
   ```csharp
   var deliveryFee = pickupBranchId != returnBranchId ? 50.0m : 0m;
   var insuranceFee = baseCost * 5.0m / 100; // 5% insurance
   ```

5. **Discounts Application**:
   ```csharp
   // Long-term discount
   if (totalDays >= 7)
   {
       var discountPercentage = totalDays >= 30 ? 15m : 10m;
       var discountAmount = subtotal * discountPercentage / 100;
   }
   ```

6. **Loyalty Points**:
   ```csharp
   // Redemption (max 50% of booking)
   var maxRedeemableValue = subtotal * 50.0m / 100;
   var loyaltyPointsDiscount = pointsToRedeem * 0.1m; // 1 point = 0.1 SAR
   
   // Earning
   var loyaltyPointsToEarn = (int)(finalAmount * 1); // 1 point per SAR
   ```

7. **Tax Calculation**:
   ```csharp
   var taxAmount = totalAfterDiscounts * 15.0m / 100; // 15% VAT
   var finalAmount = totalAfterDiscounts + taxAmount;
   ```

### **Configuration Constants**

```csharp
private const decimal TAX_PERCENTAGE = 15.0m; // 15% VAT in Saudi Arabia
private const decimal INSURANCE_PERCENTAGE = 5.0m; // 5% insurance fee
private const decimal DELIVERY_FEE = 50.0m; // Fixed delivery fee in SAR
private const int LOYALTY_POINTS_PER_SAR = 1; // 1 point per SAR spent
private const decimal LOYALTY_POINT_VALUE = 0.1m; // 1 point = 0.1 SAR
private const decimal MAX_LOYALTY_REDEMPTION_PERCENTAGE = 50.0m; // Max 50%
```

### **Business Rules**

1. **Car Availability**: Must be available for the entire rental period
2. **Long-term Discounts**: 
   - 7-29 days: 10% discount
   - 30+ days: 15% discount
3. **Loyalty Points Redemption**: Maximum 50% of booking amount
4. **Delivery Fee**: Applied only when pickup and return branches differ
5. **Insurance**: Mandatory 5% of base rental cost
6. **Tax**: 15% VAT applied to final amount after discounts

## 📱 Frontend Integration Examples

### **React/JavaScript**
```javascript
const calculateBookingCost = async (bookingData) => {
  try {
    const response = await fetch('/api/booking/calculate-cost', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}` // Optional for authenticated users
      },
      body: JSON.stringify(bookingData)
    });
    
    const result = await response.json();
    
    if (result.isAvailable) {
      displayCostBreakdown(result);
      return result;
    } else {
      showUnavailableMessage(result.unavailabilityReason);
      return null;
    }
  } catch (error) {
    console.error('Error calculating booking cost:', error);
    showErrorMessage('Failed to calculate booking cost');
  }
};

// Example usage
const bookingRequest = {
  carId: 1,
  startDate: new Date('2024-07-25T10:00:00Z'),
  endDate: new Date('2024-07-28T10:00:00Z'),
  pickupBranchId: 1,
  returnBranchId: 2,
  extraIds: [1, 2],
  loyaltyPointsToRedeem: 100
};

const costResult = await calculateBookingCost(bookingRequest);
```

### **Mobile App Integration (iOS Swift)**
```swift
struct BookingCostRequest: Codable {
    let carId: Int
    let startDate: Date
    let endDate: Date
    let pickupBranchId: Int
    let returnBranchId: Int
    let extraIds: [Int]
    let promoCode: String?
    let loyaltyPointsToRedeem: Int?
}

func calculateBookingCost(_ request: BookingCostRequest) async throws -> BookingCostResponse {
    let url = URL(string: "https://api.nolcarrental.com/api/booking/calculate-cost")!
    var urlRequest = URLRequest(url: url)
    urlRequest.httpMethod = "POST"
    urlRequest.setValue("application/json", forHTTPHeaderField: "Content-Type")
    
    if let authToken = getAuthToken() {
        urlRequest.setValue("Bearer \(authToken)", forHTTPHeaderField: "Authorization")
    }
    
    let encoder = JSONEncoder()
    encoder.dateEncodingStrategy = .iso8601
    urlRequest.httpBody = try encoder.encode(request)
    
    let (data, _) = try await URLSession.shared.data(for: urlRequest)
    
    let decoder = JSONDecoder()
    decoder.dateDecodingStrategy = .iso8601
    return try decoder.decode(BookingCostResponse.self, from: data)
}
```

### **Android Kotlin**
```kotlin
data class BookingCostRequest(
    val carId: Int,
    val startDate: String,
    val endDate: String,
    val pickupBranchId: Int,
    val returnBranchId: Int,
    val extraIds: List<Int> = emptyList(),
    val promoCode: String? = null,
    val loyaltyPointsToRedeem: Int? = null
)

suspend fun calculateBookingCost(request: BookingCostRequest): BookingCostResponse {
    return apiService.calculateBookingCost(
        authorization = "Bearer $authToken", // Optional
        request = request
    )
}

// Retrofit interface
@POST("api/booking/calculate-cost")
suspend fun calculateBookingCost(
    @Header("Authorization") authorization: String? = null,
    @Body request: BookingCostRequest
): BookingCostResponse
```

## 🔒 Security & Validation

### **Input Validation**
- All required fields validated
- Date range validation (start < end, not in past)
- Rental period limits (max 365 days)
- Positive values for IDs and loyalty points
- SQL injection protection via Entity Framework

### **Authentication**
- **Anonymous Access**: Basic cost calculation available
- **Authenticated Access**: Loyalty points features enabled
- **JWT Token**: Automatic user identification for personalized pricing

### **Error Handling**
- Comprehensive input validation
- Graceful handling of unavailable cars
- Clear error messages for all failure scenarios
- Proper HTTP status codes

## ✅ Implementation Status

- [x] Single comprehensive cost calculation endpoint
- [x] Complete cost breakdown with all components
- [x] Car availability checking integrated
- [x] Loyalty points calculation and redemption
- [x] Long-term rental discounts
- [x] Saudi Arabia tax compliance (15% VAT)
- [x] Delivery fee calculation
- [x] Insurance fee calculation
- [x] Input validation and error handling
- [x] Support for both authenticated and anonymous users
- [x] Mobile-optimized responses
- [x] Production-ready implementation

The Booking Cost Calculation API provides a **complete, production-ready solution** for calculating rental costs with a single, comprehensive endpoint that handles all pricing scenarios and business rules! 🚀💰
