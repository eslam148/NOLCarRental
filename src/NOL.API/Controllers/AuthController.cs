using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.API.Controllers;

[ApiController]
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
} 