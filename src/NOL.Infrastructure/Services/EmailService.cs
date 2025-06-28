using Microsoft.Extensions.Logging;
using NOL.Application.Common.Interfaces;
using System.Net;
using System.Net.Mail;

namespace NOL.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }
    private void sendAsync(string email, string FullName, string message)
    {
        //var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
        //{
        //    Credentials = new NetworkCredential("cae84e2257385f", "685f9ced75eb7f"),
        //    EnableSsl = true
        //};
        var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
        {
            Credentials = new NetworkCredential("75d7b89b800244", "b0b72ad703383e"),
            EnableSsl = true
        };
        client.Send("Info@nol.com", email, "Email Verification OTP", $"Email Verification OTP for {email} ({FullName}): {message}");
        // Looking to send emails in production? Check out our Email API/SMTP product!
      
      
         System.Console.WriteLine("Sent");

    }
    public async Task<bool> SendEmailVerificationOtpAsync(string email, string firstName, string otpCode)
    {
        try
        {
            // In a real implementation, you would use an email service like SendGrid, SMTP, etc.
            // For now, we'll just log the OTP code
            _logger.LogInformation($"Email Verification OTP for {email} ({firstName}): {otpCode}");
            // Looking to send emails in production? Check out our Email API/SMTP product!


            sendAsync(email, "Email Verification OTP", $"Email Verification OTP for {email} ({firstName}): {otpCode}");
           
            
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
 
            sendAsync(email, "Password Reset OTP", $"Password Reset OTP for {email} ({firstName}): {otpCode}");



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

             
            sendAsync(email, "Password Reset OTP", $"Welcome email sent to {email} ({firstName})");



            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send welcome email to {email}");
            return false;
        }
    }
} 