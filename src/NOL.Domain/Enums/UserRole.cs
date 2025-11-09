using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum UserRole
{
    [LocalizedDescription("Customer", "Customer")]
    Customer = 1,
    
    [LocalizedDescription("Employee", "Employee")]
    Employee = 2,
    
    [LocalizedDescription("BranchManager", "BranchManager")]
    BranchManager = 3,
    
    [LocalizedDescription("Admin", "Admin")]
    Admin = 4,
    
    [LocalizedDescription("SuperAdmin", "SuperAdmin")]
    SuperAdmin = 5
} 