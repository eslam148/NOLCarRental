namespace NOL.Application.Common.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailVerificationOtpAsync(string email, string firstName, string otpCode);
    Task<bool> SendPasswordResetOtpAsync(string email, string firstName, string otpCode);
    Task<bool> SendWelcomeEmailAsync(string email, string firstName);
} 