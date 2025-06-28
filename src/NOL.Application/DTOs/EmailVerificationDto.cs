namespace NOL.Application.DTOs;

public class SendEmailVerificationDto
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}

public class ResendOtpDto
{
    public string Email { get; set; } = string.Empty;
} 