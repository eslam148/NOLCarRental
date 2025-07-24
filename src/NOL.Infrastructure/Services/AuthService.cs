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
            return _responseService.Error<AuthResponseDto>("InvalidEmailORPassword");
        }
        if (!user.EmailConfirmed)
        {
            var responseResult = await SendEmailVerificationAsync(new SendEmailVerificationDto
            {
                Email = user.Email!
            });
           
            return _responseService.Success<AuthResponseDto>(new AuthResponseDto
            {
                User = new UserDto
                {
                    Email = user.Email ?? string.Empty,

                    emailVerified = user.EmailConfirmed
                },
            },
                
                "EmailNotVerified");
        }
        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            return _responseService.Error<AuthResponseDto>("InvalidEmailORPassword");
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

        return _responseService.Success(response, "LoginSuccessful");
    }

    public async Task<ApiResponse<AuthRegisterResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        // Validate required fields first
        if (string.IsNullOrEmpty(registerDto.Password) || string.IsNullOrEmpty(registerDto.ConfirmPassword))
        {
            return _responseService.ValidationError<AuthRegisterResponseDto>("PasswordRequired");
        }

        // Validate password confirmation (trim whitespace for comparison)
        if (registerDto.Password.Trim() != registerDto.ConfirmPassword.Trim())
        {
            return _responseService.ValidationError<AuthRegisterResponseDto>("PasswordsDoNotMatch");
        }

        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return _responseService.Error<AuthRegisterResponseDto>("EmailAlreadyExists");
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            PreferredLanguage = registerDto.PreferredLanguage,
            PhoneNumber = registerDto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return _responseService.Error<AuthRegisterResponseDto>("ValidationError", errors);
        }
       var responseResult =  await SendEmailVerificationAsync(new SendEmailVerificationDto
        {
            Email = user.Email!
        });
        if(!responseResult.Succeeded)
        {
            return _responseService.Error<AuthRegisterResponseDto>(responseResult.Message, responseResult.Errors);
        }
        
        var response = new AuthRegisterResponseDto
        {
             Message = "RegistrationSuccessful",
        };

        return _responseService.Success(response, "UserRegistered");
    }

    public async Task<ApiResponse> LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        return _responseService.Success("OperationSuccessful");
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
            return _responseService.NotFound("UserNotFound");
        }

        if (user.EmailConfirmed)
        {
            return _responseService.ValidationError("EmailAlreadyConfirmed");
        }

        var otpCode = GenerateOtpCode();
        user.EmailVerificationOtp = otpCode;
        user.EmailVerificationOtpExpiry = DateTime.UtcNow.AddMinutes(10); // 10 minutes expiry

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return _responseService.Error("InternalServerError");
        }

        var emailSent = await _emailService.SendEmailVerificationOtpAsync(user.Email!, user.FullName, otpCode);
        if (!emailSent)
        {
            return _responseService.Error("EmailSendingFailed");
        }

        return _responseService.Success("EmailVerificationSent");
    }

    public async Task<ApiResponse> VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return _responseService.NotFound("UserNotFound");
        }

        if (user.EmailConfirmed)
        {
            return _responseService.Error("EmailAlreadyConfirmed");
        }

        if (string.IsNullOrEmpty(user.EmailVerificationOtp) ||
            user.EmailVerificationOtpExpiry == null ||
            user.EmailVerificationOtpExpiry < DateTime.UtcNow)
        {
            return _responseService.Error("OtpExpired");
        }

        if (user.EmailVerificationOtp != dto.OtpCode)
        {
            return _responseService.Error("InvalidOtp");
        }

        user.EmailConfirmed = true;
        user.EmailVerificationOtp = null;
        user.EmailVerificationOtpExpiry = null;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return _responseService.Error("InternalServerError");
        }

        return _responseService.Success("EmailVerified");
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
            return _responseService.NotFound("UserNotFound");
        }

        var otpCode = GenerateOtpCode();
        user.PasswordResetOtp = otpCode;
        user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(15); // 15 minutes expiry

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return _responseService.Error("InternalServerError");
        }

        var emailSent = await _emailService.SendPasswordResetOtpAsync(user.Email!, user.FullName , otpCode);
        if (!emailSent)
        {
            return _responseService.Error("EmailSendingFailed");
        }

        return _responseService.Success("PasswordResetEmailSent");
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto.NewPassword.Trim() != dto.ConfirmPassword.Trim())
        {
            return _responseService.ValidationError("PasswordsDoNotMatch");
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return _responseService.NotFound("UserNotFound");
        }

        if (string.IsNullOrEmpty(user.PasswordResetOtp) ||
            user.PasswordResetOtpExpiry == null ||
            user.PasswordResetOtpExpiry < DateTime.UtcNow)
        {
            return _responseService.Error("OtpExpired");
        }

        if (user.PasswordResetOtp != dto.OtpCode)
        {
            return _responseService.Error("InvalidOtp");
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return _responseService.Error("PasswordResetFailed", errors);
        }

        user.PasswordResetOtp = null;
        user.PasswordResetOtpExpiry = null;
        await _userManager.UpdateAsync(user);

        return _responseService.Success("PasswordResetSuccessful");
    }

    public async Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        if (dto.NewPassword.Trim() != dto.ConfirmPassword.Trim())
        {
            return _responseService.ValidationError("PasswordsDoNotMatch");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return _responseService.NotFound("UserNotFound");
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return _responseService.Error("PasswordChangeFailed", errors);
        }

        return _responseService.Success("PasswordChanged");
    }

    private string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}