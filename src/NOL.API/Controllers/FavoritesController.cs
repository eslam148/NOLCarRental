using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<FavoriteDto>>>> GetMyFavorites()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _favoriteService.GetUserFavoritesAsync(userId);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FavoriteDto>>> AddToFavorites([FromBody] AddFavoriteDto addFavoriteDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _favoriteService.AddToFavoritesAsync(userId, addFavoriteDto);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpDelete("{carId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFromFavorites(int carId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _favoriteService.RemoveFromFavoritesAsync(userId, carId);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("check/{carId}")]
    public async Task<ActionResult<ApiResponse<bool>>> IsFavorite(int carId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _favoriteService.IsFavoriteAsync(userId, carId);
        return StatusCode(result.StatusCodeValue, result);
    }
} 