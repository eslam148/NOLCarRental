using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum AdvertisementStatus
{
    [LocalizedDescription("Draft", "مسودة")]
    Draft = 1,
    
    [LocalizedDescription("Active", "نشط")]
    Active = 2,
    
    [LocalizedDescription("Paused", "متوقف مؤقتاً")]
    Paused = 3,
    
    [LocalizedDescription("Expired", "منتهي الصلاحية")]
    Expired = 4,
    
    [LocalizedDescription("Canceled", "ملغي")]
    Canceled = 5
} 