using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdvertisementsController : ControllerBase
{
    private readonly IAdvertisementService _advertisementService;

    public AdvertisementsController(IAdvertisementService advertisementService)
    {
        _advertisementService = advertisementService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdvertisementDto>>>> GetActiveAdvertisements()
    {
        var result = await _advertisementService.GetActiveAdvertisementsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    //[HttpGet("featured")]
    //public async Task<ActionResult<ApiResponse<List<AdvertisementDto>>>> GetFeaturedAdvertisements()
    //{
    //    var result = await _advertisementService.GetFeaturedAdvertisementsAsync();
    //    return StatusCode(result.StatusCodeValue, result);
    //}

    //[HttpGet("car/{carId}")]
    //public async Task<ActionResult<ApiResponse<List<AdvertisementDto>>>> GetAdvertisementsByCarId(int carId)
    //{
    //    var result = await _advertisementService.GetAdvertisementsByCarIdAsync(carId);
    //    return StatusCode(result.StatusCodeValue, result);
    //}

    //[HttpGet("category/{categoryId}")]
    //public async Task<ActionResult<ApiResponse<List<AdvertisementDto>>>> GetAdvertisementsByCategoryId(int categoryId)
    //{
    //    var result = await _advertisementService.GetAdvertisementsByCategoryIdAsync(categoryId);
    //    return StatusCode(result.StatusCodeValue, result);
    //}

    //[HttpGet("{id}")]
    //public async Task<ActionResult<ApiResponse<AdvertisementDto>>> GetAdvertisement(int id)
    //{
    //    var result = await _advertisementService.GetAdvertisementByIdAsync(id);
    //    return StatusCode(result.StatusCodeValue, result);
    //}

    //[HttpPost("{id}/view")]
    //public async Task<ActionResult<ApiResponse<bool>>> IncrementViewCount(int id)
    //{
    //    var result = await _advertisementService.IncrementViewCountAsync(id);
    //    return StatusCode(result.StatusCodeValue, result);
    //}

    //[HttpPost("{id}/click")]
    //public async Task<ActionResult<ApiResponse<bool>>> IncrementClickCount(int id)
    //{
    //    var result = await _advertisementService.IncrementClickCountAsync(id);
    //    return StatusCode(result.StatusCodeValue, result);
    //}

    //[HttpPost]
    //[Authorize(Roles = "Admin,Manager")]
    //public async Task<ActionResult<ApiResponse<AdvertisementDto>>> CreateAdvertisement([FromBody] CreateAdvertisementDto createDto)
    //{
    //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //    if (string.IsNullOrEmpty(userId))
    //    {
    //        return Unauthorized();
    //    }

    //    var result = await _advertisementService.CreateAdvertisementAsync(createDto, userId);
    //    return StatusCode(result.StatusCodeValue, result);
    //}

    //[HttpPut("{id}")]
    //[Authorize(Roles = "Admin,Manager")]
    //public async Task<ActionResult<ApiResponse<AdvertisementDto>>> UpdateAdvertisement(int id, [FromBody] UpdateAdvertisementDto updateDto)
    //{
    //    var result = await _advertisementService.UpdateAdvertisementAsync(id, updateDto);
    //    return StatusCode(result.StatusCodeValue, result);
    //}

    //[HttpPut("{id}/status")]
    //[Authorize(Roles = "Admin,Manager")]
    //public async Task<ActionResult<ApiResponse<bool>>> UpdateAdvertisementStatus(int id, [FromBody] AdvertisementStatus status)
    //{
    //    var result = await _advertisementService.UpdateAdvertisementStatusAsync(id, status);
    //    return StatusCode(result.StatusCodeValue, result);
    //}

    //[HttpDelete("{id}")]
    //[Authorize(Roles = "Admin")]
    //public async Task<ActionResult<ApiResponse<bool>>> DeleteAdvertisement(int id)
    //{
    //    var result = await _advertisementService.DeleteAdvertisementAsync(id);
    //    return StatusCode(result.StatusCodeValue, result);
    //}



} 