using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NOL.Application.Common;
using NOL.Application.Common.Interfaces;
using System.Net;
using System.Net.Mail;

namespace NOL.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;

    }
    private async void sendAsync(string email, string subject, string body)
    {
        //var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
        //{
        //    Credentials = new NetworkCredential("cae84e2257385f", "685f9ced75eb7f"),
        //    EnableSsl = true
       
        // Looking to send emails in production? Check out our Email API/SMTP product!


        var maxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(2);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var messageSend = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                messageSend.To.Add(new MailAddress(email));

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = _emailSettings.EnableSsl,
                    Timeout = 30000 // 30 seconds timeout
                };

                await client.SendMailAsync(messageSend);
                _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", email, subject);
                return; // Success
            }
            catch (SmtpException ex) when (attempt < maxRetries)
            {
                _logger.LogWarning("SMTP error on attempt {Attempt}/{MaxRetries} for {Email}: {Error}",
                    attempt, maxRetries, email, ex.Message);
                await Task.Delay(retryDelay);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} after {MaxRetries} attempts", email, maxRetries);
                throw new InvalidOperationException($"Failed to send email to {email}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {Email}", email);
                throw new InvalidOperationException($"Unexpected error sending email to {email}: {ex.Message}", ex);
            }
        }


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

    public async Task<bool> SendAccountDeletionOtpAsync(string email, string firstName, string otpCode)
    {
        try
        {
            _logger.LogInformation($"Account deletion OTP sent to {email} ({firstName}): {otpCode}");

            string subject = "Account Deletion Verification - NOL Car Rental";
            string body = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>NOL Car Rental - Account Deletion Verification</title>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Arial, sans-serif;
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
                            background-color: #dc3545;
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
                        .content h2 {{
                            color: #dc3545;
                            margin-bottom: 20px;
                        }}
                        .otp-container {{
                            background-color: #f8f9fa;
                            padding: 20px;
                            border-radius: 8px;
                            text-align: center;
                            margin: 20px 0;
                            border-left: 4px solid #dc3545;
                        }}
                        .otp-code {{
                            font-size: 32px;
                            font-weight: bold;
                            color: #dc3545;
                            letter-spacing: 4px;
                            margin: 10px 0;
                            font-family: 'Courier New', monospace;
                        }}
                        .warning {{
                            background-color: #fff3cd;
                            padding: 15px;
                            border-left: 4px solid #ffc107;
                            margin: 20px 0;
                        }}
                        .footer {{
                            background-color: #f8f9fa;
                            padding: 20px;
                            text-align: center;
                            font-size: 12px;
                            color: #666666;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>NOL Car Rental</h1>
                        </div>
                        <div class='content'>
                            <h2>Account Deletion Verification</h2>
                            <p>Dear {firstName},</p>
                            <p>You have requested to permanently delete your NOL Car Rental account. To proceed with this action, please use the verification code below:</p>

                            <div class='otp-container'>
                                <p>Your account deletion verification code is:</p>
                                <div class='otp-code'>{otpCode}</div>
                                <p>This code will expire in <strong>15 minutes</strong>.</p>
                            </div>

                            <div class='warning'>
                                <p><strong>⚠️ Important Warning:</strong></p>
                                <p>This action will permanently delete your account and all associated data including:</p>
                                <ul>
                                    <li>Personal information and profile</li>
                                    <li>Booking history</li>
                                    <li>Loyalty points and rewards</li>
                                    <li>Reviews and ratings</li>
                                </ul>
                                <p><strong>This action cannot be undone!</strong></p>
                            </div>

                            <p>If you did not request account deletion, please ignore this email and contact our support team immediately.</p>

                            <p>Best regards,<br>The NOL Car Rental Team</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} NOL Car Rental. All rights reserved.</p>
                            <p>This is an automated message, please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            sendAsync(email, subject, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send account deletion OTP to {email}");
            return false;
        }
    }

    public async Task<bool> SendAccountDeletionConfirmationAsync(string email, string firstName)
    {
        try
        {
            _logger.LogInformation($"Account deletion confirmation email sent to {email} ({firstName})");

            string subject = "Account Deletion Confirmation - NOL Car Rental";
            string body = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>NOL Car Rental - Account Deletion Confirmation</title>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Arial, sans-serif;
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
                            background-color: #dc3545;
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
                        .content h2 {{
                            color: #dc3545;
                            margin-bottom: 20px;
                        }}
                        .info {{
                            background-color: #f8f9fa;
                            padding: 15px;
                            border-left: 4px solid #dc3545;
                            margin: 20px 0;
                        }}
                        .footer {{
                            background-color: #f8f9fa;
                            padding: 20px;
                            text-align: center;
                            font-size: 12px;
                            color: #666666;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>NOL Car Rental</h1>
                        </div>
                        <div class='content'>
                            <h2>Account Deletion Confirmation</h2>
                            <p>Dear {firstName},</p>
                            <p>We're writing to confirm that your NOL Car Rental account has been successfully deleted as requested.</p>

                            <div class='info'>
                                <p><strong>What this means:</strong></p>
                                <ul>
                                    <li>Your account and personal information have been permanently deleted from our system</li>
                                    <li>You will no longer receive emails from us</li>
                                    <li>Your booking history and loyalty points have been permanently removed</li>
                                    <li>You cannot log in to your account anymore</li>
                                    <li>This action cannot be undone</li>
                                </ul>
                            </div>

                            <p>If you deleted your account by mistake or would like to create a new account in the future, you can register again at any time using the same or different email address.</p>

                            <p>Thank you for being part of the NOL Car Rental community. We're sorry to see you go!</p>

                            <p>If you have any questions or concerns, please contact our support team.</p>

                            <p>Best regards,<br>The NOL Car Rental Team</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} NOL Car Rental. All rights reserved.</p>
                            <p>This is an automated message, please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            sendAsync(email, subject, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send account deletion confirmation email to {email}");
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

    public async Task<bool> SendPasswordChangeNotificationAsync(string email, string firstName)
    {
        try
        {
            _logger.LogInformation($"Password change notification email sent to {email} ({firstName})");

            string subject = "Password Changed Successfully";
            string body = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>NOL Application - Password Changed</title>
                <style>
                body {{
                    font-family: 'Segoe UI', Arial, sans-serif;
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
                    background-color: #28a745;
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
                .content h2 {{
                    color: #28a745;
                    margin-bottom: 20px;
                }}
                .success-box {{
                    background-color: #d4edda;
                    border: 1px solid #c3e6cb;
                    color: #155724;
                    padding: 20px;
                    border-radius: 8px;
                    margin: 20px 0;
                    text-align: center;
                }}
                .footer {{
                    background-color: #f8f9fa;
                    padding: 20px;
                    text-align: center;
                    font-size: 12px;
                    color: #666666;
                }}
                </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>NOL Car Rental</h1>
                        </div>
                        <div class='content'>
                            <h2>Password Changed Successfully</h2>
                            <p>Dear {firstName},</p>

                            <div class='success-box'>
                                <h3>✓ Password Successfully Changed</h3>
                                <p>Your admin account password has been changed successfully.</p>
                            </div>

                            <p>This is a confirmation that your password was changed by an administrator.</p>

                            <p>If you did not request this change, please contact your system administrator immediately.</p>

                            <p>Best regards,<br>The NOL Car Rental Team</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} NOL Car Rental. All rights reserved.</p>
                            <p>This is an automated message, please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            sendAsync(email, subject, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send password change notification email to {email}");
            return false;
        }
    }

    public async Task<bool> SendTemporaryPasswordAsync(string email, string firstName, string temporaryPassword)
    {
        try
        {
            _logger.LogInformation($"Temporary password email sent to {email} ({firstName})");

            string subject = "Temporary Password - NOL Car Rental";
            string body = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>NOL Application - Temporary Password</title>
                <style>
                body {{
                    font-family: 'Segoe UI', Arial, sans-serif;
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
                    background-color: #ffc107;
                    padding: 20px;
                    text-align: center;
                }}
                .header h1 {{
                    color: #212529;
                    margin: 0;
                    font-size: 24px;
                }}
                .content {{
                    padding: 30px 20px;
                }}
                .content h2 {{
                    color: #ffc107;
                    margin-bottom: 20px;
                }}
                .password-box {{
                    background-color: #fff3cd;
                    border: 1px solid #ffeaa7;
                    color: #856404;
                    padding: 20px;
                    border-radius: 8px;
                    margin: 20px 0;
                    text-align: center;
                    font-family: monospace;
                    font-size: 18px;
                    font-weight: bold;
                }}
                .warning {{
                    background-color: #f8d7da;
                    border: 1px solid #f5c6cb;
                    color: #721c24;
                    padding: 15px;
                    border-radius: 8px;
                    margin: 20px 0;
                }}
                .footer {{
                    background-color: #f8f9fa;
                    padding: 20px;
                    text-align: center;
                    font-size: 12px;
                    color: #666666;
                }}
                </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>NOL Car Rental</h1>
                        </div>
                        <div class='content'>
                            <h2>Temporary Password</h2>
                            <p>Dear {firstName},</p>

                            <p>Your password has been reset by an administrator. Please use the temporary password below to log in:</p>

                            <div class='password-box'>
                                {temporaryPassword}
                            </div>

                            <div class='warning'>
                                <strong>Important:</strong> Please change this temporary password immediately after logging in for security reasons.
                            </div>

                            <p>If you have any questions or need assistance, please contact your system administrator.</p>

                            <p>Best regards,<br>The NOL Car Rental Team</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} NOL Car Rental. All rights reserved.</p>
                            <p>This is an automated message, please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            sendAsync(email, subject, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send temporary password email to {email}");
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string firstName, string resetToken)
    {
        try
        {
            _logger.LogInformation($"Password reset email sent to {email} ({firstName})");

            string subject = "Password Reset Request - NOL Car Rental";
            string body = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>NOL Application - Password Reset</title>
                <style>
                body {{
                    font-family: 'Segoe UI', Arial, sans-serif;
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
                    background-color: #007bff;
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
                .content h2 {{
                    color: #007bff;
                    margin-bottom: 20px;
                }}
                .token-box {{
                    background-color: #e7f3ff;
                    border: 1px solid #b3d7ff;
                    color: #004085;
                    padding: 20px;
                    border-radius: 8px;
                    margin: 20px 0;
                    text-align: center;
                    font-family: monospace;
                    font-size: 14px;
                    word-break: break-all;
                }}
                .footer {{
                    background-color: #f8f9fa;
                    padding: 20px;
                    text-align: center;
                    font-size: 12px;
                    color: #666666;
                }}
                </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>NOL Car Rental</h1>
                        </div>
                        <div class='content'>
                            <h2>Password Reset Request</h2>
                            <p>Dear {firstName},</p>

                            <p>A password reset has been requested for your admin account. Use the reset token below if needed:</p>

                            <div class='token-box'>
                                {resetToken}
                            </div>

                            <p>If you did not request this password reset, please contact your system administrator immediately.</p>

                            <p>Best regards,<br>The NOL Car Rental Team</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} NOL Car Rental. All rights reserved.</p>
                            <p>This is an automated message, please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            sendAsync(email, subject, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send password reset email to {email}");
            return false;
        }
    }

}