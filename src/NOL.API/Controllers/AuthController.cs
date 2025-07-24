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
} 