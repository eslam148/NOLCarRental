using NOL.Domain.Attributes;

namespace NOL.Domain.Enums;

public enum PaymentMethod
{
    [LocalizedDescription("Cash", "نقدي")]
    Cash = 1,
    
    [LocalizedDescription("Credit Card", "بطاقة ائتمان")]
    CreditCard = 2,
    
    [LocalizedDescription("Debit Card", "بطاقة خصم")]
    DebitCard = 3,
    
    [LocalizedDescription("Bank Transfer", "تحويل بنكي")]
    BankTransfer = 4,
    
    [LocalizedDescription("Digital Wallet", "محفظة رقمية")]
    DigitalWallet = 5,
    
    [LocalizedDescription("Apple Pay", "آبل باي")]
    ApplePay = 6,
    
    [LocalizedDescription("Google Pay", "جوجل باي")]
    GooglePay = 7,
    
    [LocalizedDescription("STC Pay", "اس تي سي باي")]
    STCPay = 8
} 