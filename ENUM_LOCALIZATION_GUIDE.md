# Enum Localization Guide

This guide explains how to use the enum localization feature in the NOL Car Rental system.

## Overview

All enums in the system now support automatic translation between English and Arabic using data annotations. This eliminates the need for resource files and provides a cleaner, more maintainable solution.

## Components

### 1. LocalizedDescriptionAttribute

Located in: `NOL.Domain/Attributes/LocalizedDescriptionAttribute.cs`

This custom attribute is applied to enum values to store both English and Arabic translations.

```csharp
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class LocalizedDescriptionAttribute : Attribute
{
    public string EnglishName { get; set; }
    public string ArabicName { get; set; }
}
```

### 2. EnumExtensions

Located in: `NOL.Domain/Extensions/EnumExtensions.cs`

Provides extension methods to retrieve localized enum values:
- `GetLocalizedDescription()` - Gets translation based on current culture
- `GetLocalizedDescription(string culture)` - Gets translation for specific culture
- `GetAllLocalizedValues<TEnum>()` - Gets all enum values with their translations
- `GetEnumList<TEnum>()` - Gets enum list formatted for API responses

## Usage Examples

### Example 1: Get Localized Description in Code

```csharp
using NOL.Domain.Enums;
using NOL.Domain.Extensions;

// ✅ SIMPLE WAY: GetDescription() - Recommended!
var status = BookingStatus.Confirmed;
var description = status.GetDescription();
// Returns: "Confirmed" (if culture is "en")
// Returns: "مؤكد" (if culture is "ar")

// Get description for specific culture
var arabicDesc = status.GetDescription("ar");   // Returns: "مؤكد"
var englishDesc = status.GetDescription("en");  // Returns: "Confirmed"

// Alternative: GetLocalizedDescription() - Also works!
var description2 = status.GetLocalizedDescription();
var arabicDesc2 = status.GetLocalizedDescription("ar");
```

### Example 2: Use in API Controllers

```csharp
[HttpGet("statuses")]
public IActionResult GetStatuses()
{
    var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    var statuses = EnumExtensions.GetEnumList<BookingStatus>(culture);
    return Ok(statuses);
}

// API Response (English):
// [
//   { "value": 1, "name": "Open", "localizedName": "Open" },
//   { "value": 2, "name": "Confirmed", "localizedName": "Confirmed" },
//   ...
// ]

// API Response (Arabic):
// [
//   { "value": 1, "name": "Open", "localizedName": "مفتوح" },
//   { "value": 2, "name": "Confirmed", "localizedName": "مؤكد" },
//   ...
// ]
```

### Example 3: Use in Services

```csharp
public class BookingService : IBookingService
{
    public async Task<string> GetBookingStatusMessage(BookingStatus status)
    {
        // ✅ Use GetDescription() - Simple and clean!
        var localizedStatus = status.GetDescription();
        return $"Your booking status is: {localizedStatus}";
        // English: "Your booking status is: Confirmed"
        // Arabic: "Your booking status is: مؤكد"
    }
    
    public async Task SendEmailNotification(Booking booking)
    {
        var emailBody = $@"
            Dear Customer,
            
            Your booking has been {booking.Status.GetDescription()}.
            
            Car Details:
            - Status: {booking.Car.Status.GetDescription()}
            - Fuel Type: {booking.Car.FuelType.GetDescription()}
            - Transmission: {booking.Car.TransmissionType.GetDescription()}
            
            Payment: {booking.PaymentMethod.GetDescription()}
            
            Thank you!
        ";
        // All text will be in the correct language automatically!
    }
}
```

### Example 4: Display in UI (DTO)

```csharp
public class BookingDto
{
    public int Id { get; set; }
    public BookingStatus Status { get; set; }
    
    // ✅ Add localized text property
    public string StatusText => Status.GetDescription();
    
    public CarStatus CarStatus { get; set; }
    public string CarStatusText => CarStatus.GetDescription();
    
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodText => PaymentMethod.GetDescription();
}

// API Response will automatically include localized text:
// {
//   "id": 123,
//   "status": "Confirmed",
//   "statusText": "مؤكد",  // Arabic if Accept-Language: ar
//   "carStatus": "Available",
//   "carStatusText": "متاح"
// }
```

## Available Endpoints

The `EnumsController` provides endpoints to retrieve all enum values with localization:

- `GET /api/enums/booking-statuses` - All booking statuses
- `GET /api/enums/car-statuses` - All car statuses
- `GET /api/enums/fuel-types` - All fuel types
- `GET /api/enums/transmission-types` - All transmission types
- `GET /api/enums/payment-methods` - All payment methods
- `GET /api/enums/payment-statuses` - All payment statuses
- `GET /api/enums/extra-types` - All extra types
- `GET /api/enums/advertisement-statuses` - All advertisement statuses
- `GET /api/enums/advertisement-types` - All advertisement types
- `GET /api/enums/loyalty-point-transaction-types` - All loyalty point transaction types
- `GET /api/enums/loyalty-point-earn-reasons` - All loyalty point earn reasons
- `GET /api/enums/notification-types` - All notification types
- `GET /api/enums/user-roles` - All user roles
- `GET /api/enums/languages` - All languages

### 🌍 Automatic Language Detection

**The API automatically detects the language from the request header!**

The system uses ASP.NET Core's built-in localization middleware which reads the language from:

1. **Accept-Language Header** (Primary) ⭐
2. **Query String** (`?culture=ar`)
3. **Cookie** (for persistent language preference)

#### How the Magic Happens:

When a request comes in with `Accept-Language: ar`, the middleware automatically sets `CultureInfo.CurrentCulture` to Arabic. The `EnumExtensions` methods read this culture and return the appropriate translation.

**No manual language detection needed in your code!** 🎉

### Testing Localization

To test different languages, pass the `Accept-Language` header:

```bash
# English
curl -H "Accept-Language: en" http://localhost:5000/api/enums/booking-statuses

# Arabic
curl -H "Accept-Language: ar" http://localhost:5000/api/enums/booking-statuses
```

Or use the query string parameter:
```bash
http://localhost:5000/api/enums/booking-statuses?culture=ar
```

**See `test-enum-localization.http` file for comprehensive test examples!**

## Available Enums with Localization

All the following enums are fully localized:

1. **BookingStatus** - Open, Confirmed, InProgress, Completed, Canceled, Closed
2. **CarStatus** - Available, Rented, Maintenance, OutOfService
3. **FuelType** - Gasoline, Diesel, Hybrid, Electric, PluginHybrid
4. **TransmissionType** - Manual, Automatic, CVT
5. **PaymentStatus** - Pending, Processing, Success, Failed, Canceled, Refunded, PartiallyRefunded
6. **PaymentMethod** - Cash, CreditCard, DebitCard, BankTransfer, DigitalWallet, ApplePay, GooglePay, STCPay
7. **ExtraType** - GPS, ChildSeat, AdditionalDriver, Insurance, WifiHotspot, PhoneCharger, Bluetooth, RoofRack, SkiRack, BikeRack
8. **AdvertisementStatus** - Draft, Active, Paused, Expired, Canceled
9. **AdvertisementType** - Special, Discount, Seasonal, Flash, Weekend, Holiday, NewArrival, Popular
10. **LoyaltyPointTransactionType** - Earned, Redeemed, Expired, Bonus, Refund, Adjustment
11. **LoyaltyPointEarnReason** - BookingCompleted, Referral, Registration, Review, Birthday, Promotion, LongTermRental, PremiumCar
12. **NotificationType** - Booking, Payment, General, Promotion, Maintenance, Security
13. **UserRole** - Customer, Employee, BranchManager, Admin, SuperAdmin
14. **Language** - Arabic, English
15. **SettingType** - String, Number, Boolean, Decimal, Json
16. **ApiStatusCode** - Success, Created, NoContent, BadRequest, Unauthorized, Forbidden, NotFound, Conflict, UnprocessableEntity, InternalServerError, ServiceUnavailable

## Adding New Enum Values

When adding a new value to an existing enum:

1. Add the enum value
2. Apply the `[LocalizedDescription]` attribute with both English and Arabic translations

```csharp
using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum BookingStatus
{
    [LocalizedDescription("Open", "مفتوح")]
    Open = 1,
    
    // Add new value
    [LocalizedDescription("On Hold", "قيد الانتظار")]
    OnHold = 7
}
```

## Creating a New Localized Enum

1. Create your enum file in `NOL.Domain/Enums/`
2. Import the attribute: `using NOL.Domain.Attributes;`
3. Apply `[LocalizedDescription]` to each value
4. Add endpoints in `EnumsController` if needed

```csharp
using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum VehicleCondition
{
    [LocalizedDescription("New", "جديد")]
    New = 1,
    
    [LocalizedDescription("Excellent", "ممتاز")]
    Excellent = 2,
    
    [LocalizedDescription("Good", "جيد")]
    Good = 3,
    
    [LocalizedDescription("Fair", "مقبول")]
    Fair = 4
}
```

## Benefits

1. **No Resource Files** - Translations are directly on the enum values
2. **Type Safety** - Compile-time checking of enum values
3. **Easy Maintenance** - All translations in one place
4. **Automatic Culture Support** - Respects ASP.NET Core's localization middleware
5. **API-Friendly** - Easy to return localized enum lists to clients
6. **Clean Code** - Simple extension methods for easy usage

## Best Practices

1. Always provide both English and Arabic translations
2. Keep translations concise and user-friendly
3. Use the current culture context when possible
4. Cache enum lists if they're frequently accessed
5. Test with both `en` and `ar` cultures

## 📚 Quick Reference - Extension Methods

### Method 1: GetDescription() ⭐ RECOMMENDED

**Simple and clean method to get localized text**

```csharp
// Get based on current culture (from request header)
var text = myEnum.GetDescription();

// Get for specific culture
var arabicText = myEnum.GetDescription("ar");
var englishText = myEnum.GetDescription("en");
```

### Method 2: GetLocalizedDescription()

**Same as GetDescription() - use whichever you prefer**

```csharp
// Get based on current culture
var text = myEnum.GetLocalizedDescription();

// Get for specific culture
var arabicText = myEnum.GetLocalizedDescription("ar");
var englishText = myEnum.GetLocalizedDescription("en");
```

### Method 3: GetEnumList<TEnum>()

**Get all enum values as a list for API responses**

```csharp
// Get list for current culture
var list = EnumExtensions.GetEnumList<BookingStatus>();

// Get list for specific culture
var arabicList = EnumExtensions.GetEnumList<BookingStatus>("ar");

// Returns:
// [
//   { "value": 1, "name": "Open", "localizedName": "مفتوح" },
//   { "value": 2, "name": "Confirmed", "localizedName": "مؤكد" }
// ]
```

### Method 4: GetAllLocalizedValues<TEnum>()

**Get dictionary of enum values with descriptions**

```csharp
var dictionary = EnumExtensions.GetAllLocalizedValues<BookingStatus>();

// Returns Dictionary<BookingStatus, string>:
// {
//   BookingStatus.Open => "مفتوح",
//   BookingStatus.Confirmed => "مؤكد"
// }
```

## 🎯 Common Use Cases

### ✅ In DTOs
```csharp
public string StatusText => Status.GetDescription();
```

### ✅ In Services
```csharp
var message = $"Status: {booking.Status.GetDescription()}";
```

### ✅ In Controllers
```csharp
return Ok(new { status = booking.Status.GetDescription() });
```

### ✅ In Validation
```csharp
throw new Exception($"Cannot process {status.GetDescription()} booking");
```

### ✅ In Email Templates
```csharp
var body = $"Your booking is {booking.Status.GetDescription()}";
```

## 🔗 Related Files

- **Attribute**: `NOL.Domain/Attributes/LocalizedDescriptionAttribute.cs`
- **Extensions**: `NOL.Domain/Extensions/EnumExtensions.cs`
- **Controller**: `NOL.API/Controllers/EnumsController.cs`
- **Examples**: `ENUM_USAGE_EXAMPLES.cs`
- **Tests**: `QUICK_TEST_ENUMS.http`

