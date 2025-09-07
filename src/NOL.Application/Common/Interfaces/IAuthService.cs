using AutoMapper;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.Application.Common.Interfaces;

public interface IAuthService
{
    // Authentication
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    Task<ApiResponse<AuthRegisterResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponse> LogoutAsync();
    
    // Email Verification
    Task<ApiResponse> SendEmailVerificationAsync(SendEmailVerificationDto dto);
    Task<ApiResponse> VerifyEmailAsync(VerifyEmailDto dto);
    Task<ApiResponse> ResendEmailVerificationAsync(ResendOtpDto dto);
    
    // Password Management
    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto dto);
    Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordDto dto);

    // Account Management - Three-step deletion process
    Task<ApiResponse> RequestAccountDeletionAsync(string userId, RequestAccountDeletionDto dto);
    Task<ApiResponse> ConfirmAccountDeletionAsync(string userId, ConfirmAccountDeletionDto dto);
    Task<ApiResponse> ResendDeletionOtpAsync(string userId);


    //Edit User Profile

    Task<ApiResponse> EditProfile(string userId,ProfileEditDto profile);
}