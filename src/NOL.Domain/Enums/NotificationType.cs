using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum NotificationType
{
    [LocalizedDescription("Booking", "حجز")]
    Booking = 1,
    
    [LocalizedDescription("Payment", "دفع")]
    Payment = 2,
    
    [LocalizedDescription("General", "عام")]
    General = 3,
    
    [LocalizedDescription("Promotion", "ترويج")]
    Promotion = 4,
    
    [LocalizedDescription("Maintenance", "صيانة")]
    Maintenance = 5,
    
    [LocalizedDescription("Security", "أمان")]
    Security = 6
} 