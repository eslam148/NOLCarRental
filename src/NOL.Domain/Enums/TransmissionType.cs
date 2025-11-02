using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum TransmissionType
{
    [LocalizedDescription("Manual", "يدوي")]
    Manual = 1,
    
    [LocalizedDescription("Automatic", "أوتوماتيك")]
    Automatic = 2,
    
    [LocalizedDescription("CVT", "ناقل حركة متغير باستمرار")]
    CVT = 3
} 