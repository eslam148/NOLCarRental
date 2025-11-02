using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum LoyaltyPointTransactionType
{
    [LocalizedDescription("Earned", "مكتسب")]
    Earned = 1,
    
    [LocalizedDescription("Redeemed", "مستبدل")]
    Redeemed = 2,
    
    [LocalizedDescription("Expired", "منتهي الصلاحية")]
    Expired = 3,
    
    [LocalizedDescription("Bonus", "مكافأة")]
    Bonus = 4,
    
    [LocalizedDescription("Refund", "استرداد")]
    Refund = 5,
    
    [LocalizedDescription("Adjustment", "تعديل")]
    Adjustment = 6
} 