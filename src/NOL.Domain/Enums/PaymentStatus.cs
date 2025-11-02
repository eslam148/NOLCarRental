using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum PaymentStatus
{
    [LocalizedDescription("Pending", "قيد الانتظار")]
    Pending = 1,
    
    [LocalizedDescription("Processing", "قيد المعالجة")]
    Processing = 2,
    
    [LocalizedDescription("Success", "نجح")]
    Success = 3,
    
    [LocalizedDescription("Failed", "فشل")]
    Failed = 4,
    
    [LocalizedDescription("Canceled", "ملغي")]
    Canceled = 5,
    
    [LocalizedDescription("Refunded", "مسترد")]
    Refunded = 6,
    
    [LocalizedDescription("Partially Refunded", "مسترد جزئيا")]
    PartiallyRefunded = 7
} 