# 🚀 Enum Localization - Quick Cheat Sheet

## ⚡ Quick Start

```csharp
using NOL.Domain.Enums;
using NOL.Domain.Extensions;

// 1️⃣ Get localized text (simplest way)
var text = BookingStatus.Confirmed.GetDescription();
// Returns: "Confirmed" (en) or "مؤكد" (ar)

// 2️⃣ Get text for specific language
var arabicText = BookingStatus.Confirmed.GetDescription("ar");  // "مؤكد"
var englishText = BookingStatus.Confirmed.GetDescription("en"); // "Confirmed"

// 3️⃣ Get all enum values
var allStatuses = EnumExtensions.GetEnumList<BookingStatus>();
```

---

## 📋 Available Methods

| Method | Usage | When to Use |
|--------|-------|-------------|
| `GetDescription()` | `myEnum.GetDescription()` | ⭐ Most common - gets text in current language |
| `GetDescription("ar")` | `myEnum.GetDescription("ar")` | Force specific language |
| `GetEnumList<T>()` | `EnumExtensions.GetEnumList<BookingStatus>()` | API endpoints to list all values |

---

## 💡 Common Examples

### In DTOs
```csharp
public class BookingDto
{
    public BookingStatus Status { get; set; }
    public string StatusText => Status.GetDescription();
}
```

### In Services
```csharp
var message = $"Your booking is {booking.Status.GetDescription()}";
// English: "Your booking is Confirmed"
// Arabic: "Your booking is مؤكد"
```

### In Controllers
```csharp
return Ok(new {
    status = booking.Status,
    statusText = booking.Status.GetDescription()
});
```

### In Validation
```csharp
if (booking.Status == BookingStatus.Canceled)
{
    throw new Exception($"Cannot modify {booking.Status.GetDescription()} booking");
}
```

---

## 🌐 Language Detection

**Automatic!** The language is detected from:

1. **HTTP Header**: `Accept-Language: ar`
2. **Query String**: `?culture=ar`
3. **Cookie**: Persistent preference

No manual detection needed! ✅

---

## 🧪 Testing

### Using REST Client (.http file)
```http
GET https://localhost:7001/api/enums/booking-statuses
Accept-Language: ar
```

### Using cURL
```bash
curl -H "Accept-Language: ar" https://localhost:7001/api/enums/booking-statuses
```

### Using Postman
1. Add Header: `Accept-Language` = `ar`
2. Send request

---

## 📦 All Supported Enums

✅ All 16 enums are localized:

| Enum | Example Values |
|------|----------------|
| `BookingStatus` | Open, Confirmed, InProgress, Completed |
| `CarStatus` | Available, Rented, Maintenance |
| `FuelType` | Gasoline, Diesel, Hybrid, Electric |
| `TransmissionType` | Manual, Automatic, CVT |
| `PaymentMethod` | Cash, CreditCard, DebitCard, ApplePay |
| `PaymentStatus` | Pending, Success, Failed, Refunded |
| `ExtraType` | GPS, ChildSeat, Insurance, WifiHotspot |
| `AdvertisementStatus` | Draft, Active, Paused, Expired |
| `AdvertisementType` | Special, Discount, Seasonal, Flash |
| `LoyaltyPointTransactionType` | Earned, Redeemed, Expired, Bonus |
| `LoyaltyPointEarnReason` | BookingCompleted, Referral, Review |
| `NotificationType` | Booking, Payment, Promotion |
| `UserRole` | Customer, Employee, Admin, SuperAdmin |
| `Language` | Arabic, English |
| `SettingType` | String, Number, Boolean |
| `ApiStatusCode` | Success, NotFound, Unauthorized |

---

## 🎯 API Endpoints

All available at: `/api/enums/`

- `/api/enums/booking-statuses`
- `/api/enums/car-statuses`
- `/api/enums/fuel-types`
- `/api/enums/transmission-types`
- `/api/enums/payment-methods`
- `/api/enums/payment-statuses`
- `/api/enums/extra-types`
- And 7 more...

---

## ⚠️ Important Notes

1. ✅ **No resource files needed** - translations are on the enums
2. ✅ **Automatic language detection** - reads from HTTP headers
3. ✅ **Type-safe** - compile-time checking
4. ✅ **Works everywhere** - DTOs, Services, Controllers, Emails
5. ✅ **Easy to maintain** - one place for all translations

---

## 🔥 Pro Tips

```csharp
// ✅ DO: Use GetDescription() - short and simple
var text = status.GetDescription();

// ✅ DO: Add text properties to DTOs
public string StatusText => Status.GetDescription();

// ✅ DO: Use in string interpolation
$"Status: {booking.Status.GetDescription()}"

// ❌ DON'T: Convert to string directly
var text = status.ToString(); // Returns enum name, not translation!
```

---

## 📚 Full Documentation

For complete guide, see: `ENUM_LOCALIZATION_GUIDE.md`

For code examples, see: `ENUM_USAGE_EXAMPLES.cs`

For testing, use: `QUICK_TEST_ENUMS.http`

