using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum CarStatus
{
    [LocalizedDescription("Available", "متاح")]
    Available = 1,
    
    [LocalizedDescription("Rented", "مؤجر")]
    Rented = 2,
    
    [LocalizedDescription("Maintenance", "صيانة")]
    Maintenance = 3,
    
    [LocalizedDescription("Out Of Service", "خارج الخدمة")]
    OutOfService = 4
} 