using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum UserRole
{
    [LocalizedDescription("Customer", "عميل")]
    Customer = 1,
    
    [LocalizedDescription("Employee", "موظف")]
    Employee = 2,
    
    [LocalizedDescription("Branch Manager", "مدير فرع")]
    BranchManager = 3,
    
    [LocalizedDescription("Admin", "مسؤول")]
    Admin = 4,
    
    [LocalizedDescription("Super Admin", "مسؤول أعلى")]
    SuperAdmin = 5
} 