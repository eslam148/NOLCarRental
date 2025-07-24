# Booking Cost Calculator API - Single Comprehensive Endpoint

## Overview

This document describes the implementation of a single, comprehensive API endpoint for calculating booking costs in the NOL Car Rental system. The API provides complete cost breakdown including base rates, extras, fees, taxes, discounts, and loyalty points.

## 🎯 Key Features

### ✅ **Single Comprehensive Endpoint**
- **Main Endpoint**: `POST /api/bookingcalculator/calculate`
- **Quick Estimate**: `GET /api/bookingcalculator/quick-estimate`
- **Complete cost breakdown** with all fees, taxes, and discounts
- **Car availability checking** integrated
- **Loyalty points calculation** included

### ✅ **Cost Components Calculated**
1. **Base Rental Cost**: Daily rate × number of days
2. **Extras Cost**: Additional services (GPS, child seat, etc.)
3. **Delivery Fee**: If pickup and return branches are different
4. **Insurance Fee**: 5% of base cost
5. **Tax (VAT)**: 15% of final amount
6. **Discounts**: Long-term rental discounts
7. **Loyalty Points**: Redemption and earning calculation

## 🚀 API Endpoints

### **POST /api/bookingcalculator/calculate**

**Description**: Calculate complete booking cost with detailed breakdown

**Request Body**:
```json
{
  "carId": 1,
  "startDate": "2024-07-25T10:00:00Z",
  "endDate": "2024-07-28T10:00:00Z",
  "pickupBranchId": 1,
  "returnBranchId": 1,
  "extraIds": [1, 2],
  "promoCode": "SUMMER2024",
  "loyaltyPointsToRedeem": 100,
  "userId": "user123"
}
```

**Response**:
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
    }
  ],
  "totalExtrasCost": 75.00,
  
  "deliveryFee": 0.00,
  "returnFee": 0.00,
  "insuranceFee": 22.50,
  "taxAmount": 82.13,
  "taxPercentage": 15.0,
  
  "discounts": [
    {
      "discountType": "LongTermDiscount",
      "discountName": "Long Term Rental Discount (10%)",
      "discountAmount": 54.75,
      "discountPercentage": 10.0,
      "description": "Discount for 3 days rental"
    }
  ],
  "totalDiscountAmount": 54.75,
  
  "loyaltyPointsToRedeem": 100,
  "loyaltyPointsDiscount": 10.00,
  "loyaltyPointsToEarn": 547,
  
  "subTotal": 547.50,
  "totalCost": 482.75,
  "finalAmount": 564.75,
  
  "currency": "SAR",
  "isAvailable": true,
  "calculatedAt": "2024-07-24T15:30:00Z"
}
```

### **GET /api/bookingcalculator/quick-estimate**

**Description**: Quick cost estimation for initial display

**Parameters**:
- `carId` (required): Car ID
- `startDate` (required): Start date
- `endDate` (required): End date
- `extraIds` (optional): Comma-separated extra IDs

**Example**: `/api/bookingcalculator/quick-estimate?carId=1&startDate=2024-07-25&endDate=2024-07-28&extraIds=1,2`

**Response**:
```json
{
  "estimatedCost": 564.75,
  "totalDays": 3,
  "dailyRate": 150.00,
  "extrasTotal": 75.00,
  "currency": "SAR",
  "isAvailable": true,
  "message": "Estimated cost (includes taxes and fees)"
}
```

## 🔧 Implementation Details

### **Cost Calculation Logic**

1. **Base Cost Calculation**:
   ```csharp
   var totalDays = (int)Math.Ceiling((endDate - startDate).TotalDays);
   var baseCost = car.DailyRate * totalDays;
   ```

2. **Extras Calculation**:
   ```csharp
   var extrasCost = extras.Sum(e => e.DailyPrice * totalDays);
   ```

3. **Fees Calculation**:
   ```csharp
   var deliveryFee = pickupBranchId != returnBranchId ? 50.0m : 0m;
   var insuranceFee = baseCost * 5.0m / 100; // 5% insurance
   ```

4. **Discounts Application**:
   ```csharp
   // Long-term discount
   if (totalDays >= 7)
   {
       var discountPercentage = totalDays >= 30 ? 15m : 10m;
       var discountAmount = subtotal * discountPercentage / 100;
   }
   ```

5. **Tax Calculation**:
   ```csharp
   var taxAmount = totalAfterDiscounts * 15.0m / 100; // 15% VAT
   ```

6. **Loyalty Points**:
   ```csharp
   var loyaltyPointsToEarn = (int)(finalAmount * 1); // 1 point per SAR
   var loyaltyPointsDiscount = pointsToRedeem * 0.1m; // 1 point = 0.1 SAR
   ```

### **Configuration Constants**

```csharp
private const decimal TAX_PERCENTAGE = 15.0m; // 15% VAT in Saudi Arabia
private const decimal INSURANCE_PERCENTAGE = 5.0m; // 5% insurance fee
private const decimal DELIVERY_FEE = 50.0m; // Fixed delivery fee
private const int LOYALTY_POINTS_PER_SAR = 1; // 1 point per SAR spent
private const decimal LOYALTY_POINT_VALUE = 0.1m; // 1 point = 0.1 SAR
```

### **Discount Rules**

1. **Long-term Rental Discount**:
   - 7-29 days: 10% discount
   - 30+ days: 15% discount

2. **Loyalty Points Redemption**:
   - Maximum 50% of booking amount can be paid with points
   - 1 loyalty point = 0.1 SAR value

## 📱 Frontend Integration Examples

### **React/JavaScript**
```javascript
const calculateBookingCost = async (bookingData) => {
  try {
    const response = await fetch('/api/bookingcalculator/calculate', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`
      },
      body: JSON.stringify(bookingData)
    });
    
    const result = await response.json();
    
    if (result.isAvailable) {
      displayCostBreakdown(result);
    } else {
      showUnavailableMessage(result.unavailabilityReason);
    }
    
    return result;
  } catch (error) {
    console.error('Error calculating cost:', error);
  }
};

// Quick estimate for initial display
const getQuickEstimate = async (carId, startDate, endDate, extraIds = []) => {
  const params = new URLSearchParams({
    carId,
    startDate: startDate.toISOString(),
    endDate: endDate.toISOString(),
    extraIds: extraIds.join(',')
  });
  
  const response = await fetch(`/api/bookingcalculator/quick-estimate?${params}`);
  return await response.json();
};
```

### **Mobile App Integration**
```swift
// iOS Swift example
struct BookingCostRequest: Codable {
    let carId: Int
    let startDate: Date
    let endDate: Date
    let pickupBranchId: Int
    let returnBranchId: Int
    let extraIds: [Int]
    let loyaltyPointsToRedeem: Int?
}

func calculateBookingCost(_ request: BookingCostRequest) async throws -> BookingCostResponse {
    let url = URL(string: "https://api.nolcarrental.com/api/bookingcalculator/calculate")!
    var urlRequest = URLRequest(url: url)
    urlRequest.httpMethod = "POST"
    urlRequest.setValue("application/json", forHTTPHeaderField: "Content-Type")
    urlRequest.setValue("Bearer \(authToken)", forHTTPHeaderField: "Authorization")
    
    let encoder = JSONEncoder()
    encoder.dateEncodingStrategy = .iso8601
    urlRequest.httpBody = try encoder.encode(request)
    
    let (data, _) = try await URLSession.shared.data(for: urlRequest)
    
    let decoder = JSONDecoder()
    decoder.dateDecodingStrategy = .iso8601
    return try decoder.decode(BookingCostResponse.self, from: data)
}
```

## 🔒 Security & Validation

### **Input Validation**
- Car ID must be valid and exist
- Start date cannot be in the past
- End date must be after start date
- Rental period cannot exceed 365 days
- Extra IDs must be valid

### **Authentication**
- Public access for cost calculation
- Authenticated access required for loyalty points features
- User-specific pricing and discounts for logged-in users

## 🎨 Usage Scenarios

### **1. Car Listing Page**
Show quick estimates for different rental periods:
```javascript
const showPriceOptions = async (carId) => {
  const startDate = new Date();
  const estimates = await Promise.all([
    getQuickEstimate(carId, startDate, addDays(startDate, 1)),
    getQuickEstimate(carId, startDate, addDays(startDate, 3)),
    getQuickEstimate(carId, startDate, addDays(startDate, 7))
  ]);
  
  displayPriceOptions(estimates);
};
```

### **2. Booking Flow**
Calculate detailed cost before payment:
```javascript
const proceedToPayment = async () => {
  const costDetails = await calculateBookingCost(bookingData);
  
  if (costDetails.isAvailable) {
    showPaymentPage(costDetails);
  } else {
    showAlternativeCars();
  }
};
```

### **3. Cost Comparison**
Compare costs with and without extras:
```javascript
const compareWithExtras = async (baseBooking) => {
  const [withoutExtras, withExtras] = await Promise.all([
    calculateBookingCost({...baseBooking, extraIds: []}),
    calculateBookingCost(baseBooking)
  ]);
  
  showCostComparison(withoutExtras, withExtras);
};
```

## ✅ Implementation Status

- [x] Single comprehensive cost calculation endpoint
- [x] Quick estimation endpoint for initial display
- [x] Complete cost breakdown with all components
- [x] Car availability checking integrated
- [x] Loyalty points calculation and redemption
- [x] Long-term rental discounts
- [x] Tax and fee calculations
- [x] Input validation and error handling
- [x] Mobile-optimized responses
- [x] Authentication integration for user-specific features

The Booking Cost Calculator API provides a **complete, production-ready solution** for calculating rental costs with a single, comprehensive endpoint that handles all pricing scenarios! 🚀💰
