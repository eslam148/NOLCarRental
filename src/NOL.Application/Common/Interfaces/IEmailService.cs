namespace NOL.Application.Common.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailVerificationOtpAsync(string email, string firstName, string otpCode);
    Task<bool> SendPasswordResetOtpAsync(string email, string firstName, string otpCode);
    Task<bool> SendWelcomeEmailAsync(string email, string firstName);
    Task<bool> SendAccountDeletionOtpAsync(string email, string firstName, string otpCode);
    Task<bool> SendAccountDeletionConfirmationAsync(string email, string firstName);

    // Admin-specific email methods
    Task<bool> SendPasswordChangeNotificationAsync(string email, string firstName);
    Task<bool> SendTemporaryPasswordAsync(string email, string firstName, string temporaryPassword);
    Task<bool> SendPasswordResetEmailAsync(string email, string firstName, string resetToken);
}