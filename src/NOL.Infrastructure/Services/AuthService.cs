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
            return _responseService.Error<AuthResponseDto>("InvalidCredentials");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            return _responseService.Error<AuthResponseDto>("InvalidCredentials");
        }

        var token = GenerateJwtToken(user);
        var response = new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PreferredLanguage = user.PreferredLanguage,
                UserRole = user.UserRole
            }
        };

        return _responseService.Success(response, "LoginSuccessful");
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return _responseService.Error<AuthResponseDto>("EmailAlreadyExists");
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PreferredLanguage = registerDto.PreferredLanguage,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return _responseService.Error<AuthResponseDto>("ValidationError", errors);
        }

        var token = GenerateJwtToken(user);
        var response = new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PreferredLanguage = user.PreferredLanguage,
                UserRole = user.UserRole
            }
        };

        return _responseService.Success(response, "UserRegistered");
    }

    public async Task<ApiResponse> LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        return ApiResponse.Success("OperationSuccessful");
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
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
            return ApiResponse.NotFound("UserNotFound");
        }

        if (user.EmailConfirmed)
        {
            return ApiResponse.Error("EmailAlreadyConfirmed", (string?)null);
        }

        var otpCode = GenerateOtpCode();
        user.EmailVerificationOtp = otpCode;
        user.EmailVerificationOtpExpiry = DateTime.UtcNow.AddMinutes(10); // 10 minutes expiry

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return ApiResponse.Error("InternalServerError", (string?)null);
        }

        var emailSent = await _emailService.SendEmailVerificationOtpAsync(user.Email!, user.FirstName, otpCode);
        if (!emailSent)
        {
            return ApiResponse.Error("EmailSendingFailed", (string?)null);
        }

        return ApiResponse.Success("EmailVerificationSent");
    }

    public async Task<ApiResponse> VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return ApiResponse.NotFound("UserNotFound");
        }

        if (user.EmailConfirmed)
        {
            return ApiResponse.Error("EmailAlreadyConfirmed", (string?)null);
        }

        if (string.IsNullOrEmpty(user.EmailVerificationOtp) || 
            user.EmailVerificationOtpExpiry == null || 
            user.EmailVerificationOtpExpiry < DateTime.UtcNow)
        {
            return ApiResponse.Error("OtpExpired", (string?)null);
        }

        if (user.EmailVerificationOtp != dto.OtpCode)
        {
            return ApiResponse.Error("InvalidOtp", (string?)null);
        }

        user.EmailConfirmed = true;
        user.EmailVerificationOtp = null;
        user.EmailVerificationOtpExpiry = null;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return ApiResponse.Error("InternalServerError", (string?)null);
        }

        return ApiResponse.Success("EmailVerified");
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
            return ApiResponse.Success("PasswordResetEmailSent");
        }

        var otpCode = GenerateOtpCode();
        user.PasswordResetOtp = otpCode;
        user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(15); // 15 minutes expiry

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return ApiResponse.Error("InternalServerError", (string?)null);
        }

        var emailSent = await _emailService.SendPasswordResetOtpAsync(user.Email!, user.FirstName, otpCode);
        if (!emailSent)
        {
            return ApiResponse.Error("EmailSendingFailed", (string?)null);
        }

        return ApiResponse.Success("PasswordResetEmailSent");
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return ApiResponse.Error("PasswordMismatch", (string?)null);
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return ApiResponse.NotFound("UserNotFound");
        }

        if (string.IsNullOrEmpty(user.PasswordResetOtp) || 
            user.PasswordResetOtpExpiry == null || 
            user.PasswordResetOtpExpiry < DateTime.UtcNow)
        {
            return ApiResponse.Error("OtpExpired", (string?)null);
        }

        if (user.PasswordResetOtp != dto.OtpCode)
        {
            return ApiResponse.Error("InvalidOtp", (string?)null);
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse.Error("PasswordResetFailed", errors);
        }

        user.PasswordResetOtp = null;
        user.PasswordResetOtpExpiry = null;
        await _userManager.UpdateAsync(user);

        return ApiResponse.Success("PasswordResetSuccessful");
    }

    public async Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return ApiResponse.Error("PasswordMismatch", (string?)null);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse.NotFound("UserNotFound");
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse.Error("PasswordChangeFailed", errors);
        }

        return ApiResponse.Success("PasswordChanged");
    }

    private string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
} 