using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum BookingStatus
{
    [LocalizedDescription("Open", "مفتوح")]
    Open = 1,
    
    [LocalizedDescription("Confirmed", "مؤكد")]
    Confirmed = 2,
    
    [LocalizedDescription("In Progress", "قيد التنفيذ")]
    InProgress = 3,
    
    [LocalizedDescription("Completed", "مكتمل")]
    Completed = 4,
    
    [LocalizedDescription("Canceled", "ملغي")]
    Canceled = 5,
    
    [LocalizedDescription("Closed", "مغلق")]
    Closed = 6
} 