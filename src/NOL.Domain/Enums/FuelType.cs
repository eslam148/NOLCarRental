using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum FuelType
{
    [LocalizedDescription("Gasoline", "بنزين")]
    Gasoline = 1,
    
    [LocalizedDescription("Diesel", "ديزل")]
    Diesel = 2,
    
    [LocalizedDescription("Hybrid", "هجين")]
    Hybrid = 3,
    
    [LocalizedDescription("Electric", "كهربائي")]
    Electric = 4,
    
    [LocalizedDescription("Plugin Hybrid", "هجين قابل للشحن")]
    PluginHybrid = 5
} 