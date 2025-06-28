# NOL Car Rental - Loyalty Points System

## Overview
The loyalty points system allows customers to earn points for various activities and redeem them for discounts on future bookings. This system enhances customer retention and incentivizes repeat business.

## Business Rules

### Earning Points
- **1 point per $1 spent** on completed bookings
- **100 points** welcome bonus for new registrations
- **50 points** for writing reviews
- **500 points** for successful referrals
- **Points expire after 24 months** from the date they were earned
- **Minimum 100 points** required for redemption

### Point Values
- **1 point = $0.01** redemption value
- Points can be used for discounts on bookings
- No cash value; points cannot be transferred

## API Endpoints

### Customer Endpoints (Authenticated Users)

#### Get Loyalty Point Summary
```http
GET /api/loyaltypoints/summary
Authorization: Bearer {token}
```

**Response Example:**
```json
{
  "success": true,
  "data": {
    "totalLoyaltyPoints": 1250,
    "availableLoyaltyPoints": 1100,
    "lifetimePointsEarned": 1500,
    "lifetimePointsRedeemed": 400,
    "lastPointsEarnedDate": "2024-06-28T10:30:00Z",
    "pointsExpiringIn30Days": 150,
    "recentTransactions": [
      {
        "id": 1,
        "points": 125,
        "transactionType": "Earned",
        "earnReason": "BookingCompleted",
        "description": "Points earned for booking completion - $125.00",
        "transactionDate": "2024-06-28T10:30:00Z",
        "expiryDate": "2026-06-28T10:30:00Z",
        "isExpired": false,
        "bookingId": 15,
        "bookingNumber": "BK-20240628-001"
      }
    ]
  }
}
```

#### Get Transaction History
```http
GET /api/loyaltypoints/transactions?page=1&pageSize=10
Authorization: Bearer {token}
```

#### Redeem Points
```http
POST /api/loyaltypoints/redeem
Authorization: Bearer {token}
Content-Type: application/json

{
  "pointsToRedeem": 500,
  "bookingId": 123,
  "description": "Discount for summer vacation booking"
}
```

#### Calculate Discount Value
```http
GET /api/loyaltypoints/calculate-discount/500
```

**Response:**
```json
{
  "points": 500,
  "discountAmount": 5.00
}
```

#### Calculate Points for Amount
```http
GET /api/loyaltypoints/calculate-points/125.50
```

**Response:**
```json
{
  "amount": 125.50,
  "pointsEarned": 125
}
```

### Admin/Manager Endpoints

#### Award Points Manually
```http
POST /api/loyaltypoints/award
Authorization: Bearer {token}
Roles: Admin, Manager
Content-Type: application/json

{
  "userId": "user-guid-here",
  "points": 100,
  "earnReason": "Promotion",
  "description": "Holiday bonus points",
  "expiryDate": "2026-12-31T23:59:59Z"
}
```

## Database Schema

### LoyaltyPointTransactions Table
- **Id**: Primary key
- **Points**: Number of points (positive for earned, negative for redeemed)
- **TransactionType**: Earned, Redeemed, Expired, Bonus, Refund, Adjustment
- **EarnReason**: BookingCompleted, Referral, Registration, Review, Birthday, Promotion, etc.
- **Description**: Human-readable description
- **TransactionDate**: When the transaction occurred
- **ExpiryDate**: When earned points expire
- **IsExpired**: Whether points have expired
- **UserId**: Foreign key to ApplicationUser
- **BookingId**: Optional foreign key to Booking

### ApplicationUser Extended Properties
- **TotalLoyaltyPoints**: Total points ever earned
- **AvailableLoyaltyPoints**: Current available points for redemption
- **LifetimePointsEarned**: Total points earned in lifetime
- **LifetimePointsRedeemed**: Total points redeemed in lifetime
- **LastPointsEarnedDate**: Date of last point earning activity

## Integration Points

### Booking Completion
When a booking is completed (status changed to Completed), the system automatically:
1. Calculates points based on the final booking amount
2. Creates a loyalty point transaction
3. Updates user's point totals
4. Prevents duplicate point awards for the same booking

### User Registration
New users receive a 100-point welcome bonus upon successful registration.

### Review System
Users earn 50 points when they submit a review for a completed booking.

## Localization Support

The system supports both English and Arabic messages:

### English Messages
- "Loyalty point summary retrieved successfully"
- "Loyalty points awarded successfully"
- "Loyalty points redeemed successfully"
- "Minimum redemption amount not met. You need at least 100 points to redeem."
- "Insufficient loyalty points for this redemption"

### Arabic Messages
- "تم استرداد ملخص نقاط الولاء بنجاح"
- "تم منح نقاط الولاء بنجاح"
- "تم استبدال نقاط الولاء بنجاح"
- "لم يتم استيفاء الحد الأدنى للاستبدال. تحتاج إلى 100 نقطة على الأقل للاستبدال."
- "نقاط ولاء غير كافية لهذا الاستبدال"

## Future Enhancements

### Potential Features
1. **Tier System**: Bronze, Silver, Gold tiers with different earning rates
2. **Bonus Multipliers**: Double points for premium car categories
3. **Expiration Notifications**: Email alerts for points expiring soon
4. **Gift Points**: Allow users to transfer points to friends/family
5. **Seasonal Promotions**: Temporary bonus point campaigns
6. **Point History Export**: Allow users to download transaction history
7. **Mobile Push Notifications**: Real-time point earning notifications

### Background Jobs
- **Point Expiration**: Daily job to expire old points
- **Tier Recalculation**: Monthly job to update user tiers
- **Expiration Reminders**: Weekly job to notify users of expiring points

## Usage Examples

### Earning Points Scenario
1. Customer makes a $150 booking
2. Upon booking completion, system awards 150 points
3. Points expire in 24 months (June 2026)
4. Customer receives confirmation notification

### Redemption Scenario
1. Customer has 500 available points
2. Customer applies 400 points to new booking
3. System provides $4.00 discount
4. Customer retains 100 points
5. Transaction recorded as "Redeemed"

### Admin Bonus Scenario
1. Admin awards 200 bonus points for holiday promotion
2. Points added to customer account immediately
3. Marked as "Promotion" type with custom expiry
4. Customer notified of bonus points

This loyalty points system provides a comprehensive foundation for customer retention while maintaining clean architecture principles and supporting the bilingual requirements of the NOL Car Rental platform. 