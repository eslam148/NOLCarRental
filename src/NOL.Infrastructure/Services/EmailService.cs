using NOL.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace NOL.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendEmailVerificationOtpAsync(string email, string firstName, string otpCode)
    {
        try
        {
            // In a real implementation, you would use an email service like SendGrid, SMTP, etc.
            // For now, we'll just log the OTP code
            _logger.LogInformation($"Email Verification OTP for {email} ({firstName}): {otpCode}");
            
            // Simulate email sending delay
            await Task.Delay(100);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email verification OTP to {email}");
            return false;
        }
    }

    public async Task<bool> SendPasswordResetOtpAsync(string email, string firstName, string otpCode)
    {
        try
        {
            // In a real implementation, you would use an email service like SendGrid, SMTP, etc.
            // For now, we'll just log the OTP code
            _logger.LogInformation($"Password Reset OTP for {email} ({firstName}): {otpCode}");
            
            // Simulate email sending delay
            await Task.Delay(100);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send password reset OTP to {email}");
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
    {
        try
        {
            // In a real implementation, you would send a welcome email
            _logger.LogInformation($"Welcome email sent to {email} ({firstName})");
            
            // Simulate email sending delay
            await Task.Delay(100);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send welcome email to {email}");
            return false;
        }
    }
} 