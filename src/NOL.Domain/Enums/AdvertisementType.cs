using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum AdvertisementType
{
    [LocalizedDescription("Special", "خاص")]
    Special = 1,
    
    [LocalizedDescription("Discount", "خصم")]
    Discount = 2,
    
    [LocalizedDescription("Seasonal", "موسمي")]
    Seasonal = 3,
    
    [LocalizedDescription("Flash", "فلاش")]
    Flash = 4,
    
    [LocalizedDescription("Weekend", "عطلة نهاية الأسبوع")]
    Weekend = 5,
    
    [LocalizedDescription("Holiday", "عطلة")]
    Holiday = 6,
    
    [LocalizedDescription("New Arrival", "جديد")]
    NewArrival = 7,
    
    [LocalizedDescription("Popular", "شائع")]
    Popular = 8
} 