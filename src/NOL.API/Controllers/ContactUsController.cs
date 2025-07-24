using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.DTOs;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactUsController : ControllerBase
{
    private readonly IContactUsService _contactUsService;

    public ContactUsController(IContactUsService contactUsService)
    {
        _contactUsService = contactUsService;
    }

    /// <summary>
    /// Get active contact us information (Public endpoint)
    /// </summary>
    /// <returns>Active contact us information</returns>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveContactUs()
    {
        var result = await _contactUsService.GetActiveContactUsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get all contact us entries (Admin only)
    /// </summary>
    /// <returns>List of all contact us entries</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllContactUs()
    {
        var result = await _contactUsService.GetAllContactUsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get contact us by ID (Admin only)
    /// </summary>
    /// <param name="id">Contact us ID</param>
    /// <returns>Contact us details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetContactUsById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid contact us ID" });
        }

        var result = await _contactUsService.GetContactUsByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Create new contact us entry (Admin only)
    /// </summary>
    /// <param name="createContactUsDto">Contact us creation data</param>
    /// <returns>Created contact us</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateContactUs([FromBody] CreateContactUsDto createContactUsDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _contactUsService.CreateContactUsAsync(createContactUsDto);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update contact us entry (Admin only)
    /// </summary>
    /// <param name="id">Contact us ID</param>
    /// <param name="updateContactUsDto">Contact us update data</param>
    /// <returns>Updated contact us</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateContactUs(int id, [FromBody] UpdateContactUsDto updateContactUsDto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid contact us ID" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _contactUsService.UpdateContactUsAsync(id, updateContactUsDto);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete contact us entry (Admin only)
    /// </summary>
    /// <param name="id">Contact us ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteContactUs(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid contact us ID" });
        }

        var result = await _contactUsService.DeleteContactUsAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Set contact us entry as active (Admin only)
    /// </summary>
    /// <param name="id">Contact us ID</param>
    /// <returns>Activation result</returns>
    [HttpPost("{id}/set-active")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetActiveContactUs(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid contact us ID" });
        }

        var result = await _contactUsService.SetActiveContactUsAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get total count of contact us entries (Admin only)
    /// </summary>
    /// <returns>Total count</returns>
    [HttpGet("count")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTotalContactUsCount()
    {
        var result = await _contactUsService.GetTotalContactUsCountAsync();
        return StatusCode(result.StatusCodeValue, result);
    }
}
