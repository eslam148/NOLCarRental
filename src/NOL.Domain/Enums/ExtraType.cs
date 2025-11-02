using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum ExtraType
{
    [LocalizedDescription("GPS", "نظام تحديد المواقع")]
    GPS = 1,
    
    [LocalizedDescription("Child Seat", "مقعد أطفال")]
    ChildSeat = 2,
    
    [LocalizedDescription("Additional Driver", "سائق إضافي")]
    AdditionalDriver = 3,
    
    [LocalizedDescription("Insurance", "تأمين")]
    Insurance = 4,
    
    [LocalizedDescription("WiFi Hotspot", "نقطة واي فاي")]
    WifiHotspot = 5,
    
    [LocalizedDescription("Phone Charger", "شاحن هاتف")]
    PhoneCharger = 6,
    
    [LocalizedDescription("Bluetooth", "بلوتوث")]
    Bluetooth = 7,
    
    [LocalizedDescription("Roof Rack", "حامل السقف")]
    RoofRack = 8,
    
    [LocalizedDescription("Ski Rack", "حامل التزلج")]
    SkiRack = 9,
    
    [LocalizedDescription("Bike Rack", "حامل الدراجات")]
    BikeRack = 10
} 