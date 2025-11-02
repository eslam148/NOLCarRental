using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum LoyaltyPointEarnReason
{
    [LocalizedDescription("Booking Completed", "اكتمال الحجز")]
    BookingCompleted = 1,
    
    [LocalizedDescription("Referral", "إحالة")]
    Referral = 2,
    
    [LocalizedDescription("Registration", "التسجيل")]
    Registration = 3,
    
    [LocalizedDescription("Review", "تقييم")]
    Review = 4,
    
    [LocalizedDescription("Birthday", "عيد ميلاد")]
    Birthday = 5,
    
    [LocalizedDescription("Promotion", "ترويج")]
    Promotion = 6,
    
    [LocalizedDescription("Long Term Rental", "إيجار طويل الأجل")]
    LongTermRental = 7,
    
    [LocalizedDescription("Premium Car", "سيارة فاخرة")]
    PremiumCar = 8
} 