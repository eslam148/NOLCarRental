using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.API.Controllers;

[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        var result = await _authService.LogoutAsync();
        return StatusCode((int)result.StatusCode, result);
    }

    // Email Verification Endpoints
    [HttpPost("send-email-verification")]
    public async Task<ActionResult<ApiResponse>> SendEmailVerification([FromBody] SendEmailVerificationDto dto)
    {
        var result = await _authService.SendEmailVerificationAsync(dto);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiResponse>> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        var result = await _authService.VerifyEmailAsync(dto);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpPost("resend-email-verification")]
    public async Task<ActionResult<ApiResponse>> ResendEmailVerification([FromBody] ResendOtpDto dto)
    {
        var result = await _authService.ResendEmailVerificationAsync(dto);
        return StatusCode(result.StatusCodeValue, result);
    }

    // Password Management Endpoints
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await _authService.ForgotPasswordAsync(dto);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePasswordAsync(userId, dto);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Step 1: Request account deletion - Send OTP to user's email
    /// </summary>
    /// <param name="dto">Account deletion request with password verification</param>
    /// <returns>Confirmation that OTP has been sent</returns>
    [HttpPost("request-account-deletion")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> RequestAccountDeletion([FromBody] RequestAccountDeletionDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.RequestAccountDeletionAsync(userId, dto);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Step 2: Confirm account deletion - Verify OTP and permanently delete account
    /// </summary>
    /// <param name="dto">OTP verification and confirmation text</param>
    /// <returns>Confirmation of account deletion</returns>
    [HttpPost("confirm-account-deletion")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ConfirmAccountDeletion([FromBody] ConfirmAccountDeletionDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.ConfirmAccountDeletionAsync(userId, dto);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Step 3: Resend account deletion OTP if user didn't receive it
    /// </summary>
    /// <returns>Confirmation that new OTP has been sent</returns>
    [HttpPost("resend-deletion-otp")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ResendDeletionOtp()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.ResendDeletionOtpAsync(userId);
        return StatusCode(result.StatusCodeValue, result);
    }
}