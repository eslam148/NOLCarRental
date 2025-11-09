using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Models;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Domain.Extensions;

namespace NOL.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly LocalizedApiResponseService _responseService;
    private readonly IEmailService _emailService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtSettings jwtSettings,
        LocalizedApiResponseService responseService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings;
        _responseService = responseService;
        _emailService = emailService;
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return _responseService.Error<AuthResponseDto>(ResponseCode.InvalidEmailOrPassword);
        }
  
        if (!user.EmailConfirmed)
        {
            // Send verification email automatically
            await SendEmailVerificationAsync(new SendEmailVerificationDto
            {
                Email = user.Email!
            });

            // Return error response indicating email verification is required
            return _responseService.Error<AuthResponseDto>(ResponseCode.EmailNotVerified,
                statusCode: ApiStatusCode.Unauthorized);
        }
        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            return _responseService.Error<AuthResponseDto>(ResponseCode.InvalidDateRange);
        }

     
        var token = GenerateJwtToken(user);
        var response = new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                loyaltyPoints = user.TotalLoyaltyPoints,
                PreferredLanguage = user.PreferredLanguage,
                UserRole = user.UserRole,
                emailVerified = user.EmailConfirmed
            }
        };

        return _responseService.Success(response, ResponseCode.LoginSuccessful);
    }

    public async Task<ApiResponse<AuthRegisterResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        // Validate required fields first
        if (string.IsNullOrEmpty(registerDto.Password) || string.IsNullOrEmpty(registerDto.ConfirmPassword))
        {
            return _responseService.ValidationError<AuthRegisterResponseDto>(ResponseCode.PasswordRequired);
        }

        // Validate password confirmation (trim whitespace for comparison)
        if (registerDto.Password.Trim() != registerDto.ConfirmPassword.Trim())
        {
            return _responseService.ValidationError<AuthRegisterResponseDto>(ResponseCode.PasswordsDoNotMatch);
        }

        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return _responseService.Error<AuthRegisterResponseDto>(ResponseCode.EmailAlreadyExists);
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            PreferredLanguage = registerDto.PreferredLanguage,
            PhoneNumber = registerDto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserRole = UserRole.Customer
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            // var errors = result.Errors.Select(e => e.Description).ToList();
            return _responseService.Error<AuthRegisterResponseDto>(ResponseCode.ValidationError);
        }
       var addRoleResut = await  _userManager.AddToRoleAsync(user, UserRole.Customer.GetDescription());
       
       var responseResult =  await SendEmailVerificationAsync(new SendEmailVerificationDto
        {
            Email = user.Email!
        });
        if(!responseResult.Succeeded)
        {
            return _responseService.Error<AuthRegisterResponseDto>(ResponseCode.ValidationError);
        }
        
        var response = new AuthRegisterResponseDto
        {
             Message = "RegistrationSuccessful",
        };

        return _responseService.Success(response, ResponseCode.UserRegistered);
    }

    public async Task<ApiResponse> LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        return _responseService.Success(ResponseCode.OperationSuccessful);
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, $"{user.FullName}"),
            new("preferredLanguage", user.PreferredLanguage.ToString()),
            new(ClaimTypes.Role, user.UserRole.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // Email Verification Methods
    public async Task<ApiResponse> SendEmailVerificationAsync(SendEmailVerificationDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return _responseService.NotFound(ResponseCode.UserNotFound);
        }

        if (user.EmailConfirmed)
        {
            return _responseService.ValidationError(ResponseCode.EmailAlreadyConfirmed);
        }

        var otpCode = GenerateOtpCode();
        user.EmailVerificationOtp = otpCode;
        user.EmailVerificationOtpExpiry = DateTime.UtcNow.AddMinutes(10); // 10 minutes expiry

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return _responseService.Error(ResponseCode.InternalServerError);
        }

        var emailSent = await _emailService.SendEmailVerificationOtpAsync(user.Email!, user.FullName, otpCode);
        if (!emailSent)
        {
            //return _responseService.Error("EmailSendingFailed");
        }

        return _responseService.Success(ResponseCode.EmailVerificationSent);
    }

    public async Task<ApiResponse> VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return _responseService.NotFound(ResponseCode.UserNotFound);
        }

        if (user.EmailConfirmed)
        {
            return _responseService.Error(ResponseCode.EmailAlreadyConfirmed);
        }

        if (string.IsNullOrEmpty(user.EmailVerificationOtp) ||
            user.EmailVerificationOtpExpiry == null ||
            user.EmailVerificationOtpExpiry < DateTime.UtcNow)
        {
            return _responseService.Error(ResponseCode.OtpExpired);
        }

        if (user.EmailVerificationOtp != dto.OtpCode)
        {
            return _responseService.Error(ResponseCode.InvalidOtp);
        }

        user.EmailConfirmed = true;
        user.EmailVerificationOtp = null;
        user.EmailVerificationOtpExpiry = null;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return _responseService.Error(ResponseCode.InternalServerError);
        }

        return _responseService.Success(ResponseCode.EmailVerified);
    }

    public async Task<ApiResponse> ResendEmailVerificationAsync(ResendOtpDto dto)
    {
        return await SendEmailVerificationAsync(new SendEmailVerificationDto { Email = dto.Email });
    }

    // Password Management Methods
    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            // Don't reveal that user doesn't exist
            return _responseService.NotFound(ResponseCode.UserNotFound);
        }

        var otpCode = GenerateOtpCode();
        user.PasswordResetOtp = otpCode;
        user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(15); // 15 minutes expiry

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return _responseService.Error(ResponseCode.InternalServerError);
        }

        var emailSent = await _emailService.SendPasswordResetOtpAsync(user.Email!, user.FullName , otpCode);
        if (!emailSent)
        {
           // return _responseService.Error("EmailSendingFailed");
        }

        return _responseService.Success(ResponseCode.PasswordResetEmailSent);
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto.NewPassword.Trim() != dto.ConfirmPassword.Trim())
        {
            return _responseService.ValidationError(ResponseCode.PasswordsDoNotMatch);
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return _responseService.NotFound(ResponseCode.UserNotFound);
        }

        if (string.IsNullOrEmpty(user.PasswordResetOtp) ||
            user.PasswordResetOtpExpiry == null ||
            user.PasswordResetOtpExpiry < DateTime.UtcNow)
        {
            return _responseService.Error(ResponseCode.OtpExpired);
        }

        if (user.PasswordResetOtp != dto.OtpCode)
        {
            return _responseService.Error(ResponseCode.InvalidOtp);
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return _responseService.Error(ResponseCode.PasswordResetFailed);
        }

        user.PasswordResetOtp = null;
        user.PasswordResetOtpExpiry = null;
        await _userManager.UpdateAsync(user);

        return _responseService.Success(ResponseCode.PasswordResetSuccessful);
    }

    public async Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        if (dto.NewPassword.Trim() != dto.ConfirmPassword.Trim())
        {
            return _responseService.ValidationError(ResponseCode.PasswordsDoNotMatch);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return _responseService.NotFound(ResponseCode.UserNotFound);
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
             return _responseService.Error(ResponseCode.PasswordChangeFailed);
        }

        return _responseService.Success(ResponseCode.PasswordChanged);
    }

    // Account Management Methods - Three-step deletion process
    public async Task<ApiResponse> RequestAccountDeletionAsync(string userId, RequestAccountDeletionDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return _responseService.NotFound(ResponseCode.UserNotFound);
        }

        // Verify current password
        var passwordCheck = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
        if (!passwordCheck)
        {
            return _responseService.ValidationError(ResponseCode.InvalidPassword);
        }

         
            // Generate OTP for account deletion
            var otpCode = GenerateOtpCode();
            user.AccountDeletionOtp = otpCode;
            user.AccountDeletionOtpExpiry = DateTime.UtcNow.AddMinutes(15); // 15 minutes expiry
            user.AccountDeletionOtpResendCount = 0; // Reset resend count
            user.LastAccountDeletionOtpResendTime = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return _responseService.Error(ResponseCode.InternalServerError);
            }

            // Send account deletion OTP email
            var emailSent = await _emailService.SendAccountDeletionOtpAsync(user.Email!, user.FullName, otpCode);
            if (!emailSent)
            {
                return _responseService.Error(ResponseCode.EmailSendingFailed);
            }

            return _responseService.Success(ResponseCode.AccountDeletionOtpSent);
        
    }

    public async Task<ApiResponse> ConfirmAccountDeletionAsync(string userId, ConfirmAccountDeletionDto dto)
    {
        // Validate confirmation text
        if (dto.ConfirmationText?.Trim().ToUpper() != "DELETE")
        {
            return _responseService.ValidationError(ResponseCode.InvalidConfirmationText);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return _responseService.NotFound(ResponseCode.UserNotFound);
        }

        // Validate OTP
        if (string.IsNullOrEmpty(user.AccountDeletionOtp) ||
            user.AccountDeletionOtpExpiry == null ||
            user.AccountDeletionOtpExpiry < DateTime.UtcNow)
        {
            return _responseService.Error(ResponseCode.OtpExpired);
        }

        if (user.AccountDeletionOtp != dto.OtpCode)
        {
            return _responseService.Error(ResponseCode.OtpExpired);
        }

       
            // Store user info for confirmation email before deletion
            var userEmail = user.Email!;
            var userFullName = user.FullName;

            // Sign out the user from all devices first
            await _signInManager.SignOutAsync();

            // Hard delete - permanently remove the user account
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                // 0  var errors = deleteResult.Errors.Select(e => e.Description).ToList();
                return _responseService.Error(ResponseCode.AccountDeletionFailed);
            }

            // Send account deletion confirmation email
            try
            {
                await _emailService.SendAccountDeletionConfirmationAsync(userEmail, userFullName);
            }
            catch
            {
                // Don't fail the deletion if email sending fails
            }

            return _responseService.Success(ResponseCode.AccountDeletedSuccessfully);
         
    }

    public async Task<ApiResponse> ResendDeletionOtpAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return _responseService.NotFound(ResponseCode.UserNotFound);
        }

        // Check rate limiting - max 3 resends per hour
        if (user.LastAccountDeletionOtpResendTime.HasValue)
        {
            var timeSinceLastResend = DateTime.UtcNow - user.LastAccountDeletionOtpResendTime.Value;
            if (timeSinceLastResend.TotalHours < 1 && user.AccountDeletionOtpResendCount >= 3)
            {
                return _responseService.Error(ResponseCode.TooManyResendAttempts);
            }

            // Reset count if more than an hour has passed
            if (timeSinceLastResend.TotalHours >= 1)
            {
                user.AccountDeletionOtpResendCount = 0;
            }
        }

        // Check if there's an active deletion request
        if (string.IsNullOrEmpty(user.AccountDeletionOtp) ||
            user.AccountDeletionOtpExpiry == null ||
            user.AccountDeletionOtpExpiry < DateTime.UtcNow)
        {
            return _responseService.Error(ResponseCode.NoDeletionRequestFound);
        }

       
            // Generate new OTP
            var otpCode = GenerateOtpCode();
            user.AccountDeletionOtp = otpCode;
            user.AccountDeletionOtpExpiry = DateTime.UtcNow.AddMinutes(15); // 15 minutes expiry
            user.AccountDeletionOtpResendCount++;
            user.LastAccountDeletionOtpResendTime = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return _responseService.Error(ResponseCode.InternalServerError);
            }

            // Send new OTP email
            var emailSent = await _emailService.SendAccountDeletionOtpAsync(user.Email!, user.FullName, otpCode);
            if (!emailSent)
            {
                return _responseService.Error(ResponseCode.EmailSendingFailed);
            }

            return _responseService.Success(ResponseCode.AccountDeletionOtpResent);
        
    }

    public async Task<ApiResponse> EditProfile(string userId, ProfileEditDto profileDto)
    {
         
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return _responseService.NotFound(ResponseCode.UserNotFound);
            }

            // Update user profile fields (excluding email)
            user.FullName = profileDto.FullName;
            user.PhoneNumber = profileDto.PhoneNumber;
            user.PreferredLanguage = profileDto.PreferredLanguage;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
              //  var errors = result.Errors.Select(e => e.Description).ToList();
                return _responseService.Error(ResponseCode.ProfileUpdateFailed);
            }

            return _responseService.Success(ResponseCode.ProfileUpdatedSuccessfully);
        
    }

    private string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}