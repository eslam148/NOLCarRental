using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum SettingType
{
    [LocalizedDescription("String", "نص")]
    String = 1,
    
    [LocalizedDescription("Number", "رقم")]
    Number = 2,
    
    [LocalizedDescription("Boolean", "منطقي")]
    Boolean = 3,
    
    [LocalizedDescription("Decimal", "عشري")]
    Decimal = 4,
    
    [LocalizedDescription("JSON", "جيسون")]
    Json = 5
} 