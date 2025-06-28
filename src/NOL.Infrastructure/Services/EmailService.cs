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
    private void sendAsync(string email, string subject, string message)
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

        client.Send("Info@nol.com", email, subject, message);
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


             
            SendOtpEmailAsync(email, otpCode, false);

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

            
            SendOtpEmailAsync(email, otpCode, true);


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



    public   void SendOtpEmailAsync(string email, string otp, bool isPasswordReset)
    {
        string subject = isPasswordReset ? "Password Reset OTP" : "Email Verification OTP";
        string purpose = isPasswordReset ? "reset your password" : "verify your email";

        string body = $@"
                 <!DOCTYPE html>
                  <html lang='en'>
                  <head>
                  <meta charset='UTF-8'>
                  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                  <title>NOL Application - {subject}</title>
                  <style>
                   body {{
                    font-family: 'Segoe UI', Arial, sans-serif;                        \
                    line-height: 1.6;
                    color: #333333;
                    margin: 0;
                    padding: 0;
                    background-color: #f4f4f4;
                    }}
                    .container {{
                        width: 100%;
                        max-width: 600px;
                        margin: 0 auto;
                        background-color: #ffffff;
                        border-radius: 8px;
                        overflow: hidden;
                        box-shadow: 0 3px 10px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            background-color: #0056b3;
                            padding: 20px;
                            text-align: center;
                            }}
                            .header h1 {{
                                color: #ffffff;
                                margin: 0;
                                font-size: 24px;
                                }}
                                .content {{
                                    padding: 30px 20px;

                                    }}
                                    .otp-container {{
                                    text-align: center;
                                    margin: 30px 0;
                                     padding: 10px;
                                     background-color: #f8f9fa;
                                     border-radius: 4px;
                                     }}
                                     .otp-code {{
                                     font-family: monospace;
                                     font-size: 28px;
                                     letter-spacing: 5px;
                                     font-weight: bold;
                                     color: #0056b3;
                                     }}
                                     .info {{
                                     color: #666666;
                                     font-size: 14px;
                                     margin-top: 20px;
                                     }}
                                     .footer {{
                                     text-align: center;
                                     padding: 15px 20px;
                                     background-color: #f8f9fa;
                                     font-size: 12px;
                                     color: #888888;
                                     }}
                                     .logo {{
                                     max-height: 50px;
                                     margin-bottom: 10px;
                                     }}
                                     </style>
                                     </head>
                                     <body>
                                     <div class='container'>
                                     <div class='header'>
                                     <!-- <img src='logo-url' alt='NOL Logo' class='logo'> -->
                                     <h1>NOL Application</h1>
                                     </div>
                                     <div class='content'>
                                     <h2>{subject}</h2>
                                     <p>Thank you for using NOL Application. We need to verify your identity.</p>
                                     <div class='otp-container'>
                                     <p>Your verification code to {purpose} is:</p>
                                     <div class='otp-code'>{otp}</div>
                                     <p>This code will expire in <strong>10 minutes</strong>.</p>
                                     </div>
                                     <p class='info'>If you did not request this code, please ignore this email or contact our support team if you believe this is suspicious activity.</p>
                                     </div>
                                     <div class='footer'>
                                     <p>&copy; {DateTime.Now.Year} NOL Application. All rights reserved.</p>
                                     <p>This is an automated message, please do not reply to this email.</p>
                                     </div>
                                     </div>
                                     </body>
                                     </html>";

          sendAsync(email, subject, body);
    }

}