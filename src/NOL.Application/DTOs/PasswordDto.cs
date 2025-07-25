namespace NOL.Application.DTOs;

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

// Three-step account deletion DTOs
public class RequestAccountDeletionDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string? Reason { get; set; } // Optional reason for deletion
}

public class ConfirmAccountDeletionDto
{
    public string OtpCode { get; set; } = string.Empty;
    public string ConfirmationText { get; set; } = string.Empty; // User must type "DELETE" to confirm
}

public class ResendDeletionOtpDto
{
    // No additional fields needed - user ID comes from JWT token
}