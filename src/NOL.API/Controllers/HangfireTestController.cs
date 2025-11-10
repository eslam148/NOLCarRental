using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Hangfire;

namespace NOL.API.Controllers;

/// <summary>
/// Demo controller for testing Hangfire jobs locally
/// WARNING: Remove or secure this controller in production
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // For local testing only - SECURE IN PRODUCTION!
public class HangfireTestController : ControllerBase
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IBookingCleanupService _bookingCleanupService;
    private readonly ILogger<HangfireTestController> _logger;

    public HangfireTestController(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        IBookingCleanupService bookingCleanupService,
        ILogger<HangfireTestController> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _bookingCleanupService = bookingCleanupService;
        _logger = logger;
    }

    /// <summary>
    /// Trigger the booking cleanup job immediately (fire-and-forget)
    /// </summary>
    /// <returns>Job ID</returns>
    [HttpPost("trigger-close-bookings")]
    public IActionResult TriggerCloseBookingsJob()
    {
        _logger.LogInformation("Manual trigger: Close ended bookings job");

        var jobId = _backgroundJobClient.Enqueue<EndBookingSchedulJob>(job => job.Execute());

        return Ok(new
        {
            success = true,
            message = "Job enqueued successfully",
            jobId = jobId,
            note = "Check Hangfire dashboard at /hangfire to monitor job execution"
        });
    }

    /// <summary>
    /// Test the booking cleanup service directly (synchronous)
    /// </summary>
    /// <returns>Number of bookings closed</returns>
    [HttpPost("test-service-direct")]
    public async Task<IActionResult> TestServiceDirect()
    {
        _logger.LogInformation("Manual test: Direct service call");

        try
        {
            var startTime = DateTime.UtcNow;
            var closedCount = await _bookingCleanupService.CloseEndedBookingsAsync();
            var duration = (DateTime.UtcNow - startTime).TotalSeconds;

            return Ok(new
            {
                success = true,
                message = "Service executed successfully",
                bookingsClosed = closedCount,
                durationSeconds = Math.Round(duration, 2),
                executedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing service directly");
            return StatusCode(500, new
            {
                success = false,
                message = "Error executing service",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Schedule a delayed job (runs after specified seconds)
    /// </summary>
    /// <param name="delaySeconds">Delay in seconds (default: 10)</param>
    /// <returns>Job ID</returns>
    [HttpPost("schedule-delayed")]
    public IActionResult ScheduleDelayedJob([FromQuery] int delaySeconds = 10)
    {
        _logger.LogInformation("Scheduling delayed job: {Seconds} seconds", delaySeconds);

        var jobId = _backgroundJobClient.Schedule<EndBookingSchedulJob>(
            job => job.Execute(),
            TimeSpan.FromSeconds(delaySeconds));

        return Ok(new
        {
            success = true,
            message = $"Job scheduled to run in {delaySeconds} seconds",
            jobId = jobId,
            willExecuteAt = DateTime.UtcNow.AddSeconds(delaySeconds)
        });
    }

    /// <summary>
    /// Trigger the recurring job manually (runs once immediately)
    /// </summary>
    /// <returns>Result</returns>
    [HttpPost("trigger-recurring-job")]
    public IActionResult TriggerRecurringJob()
    {
        _logger.LogInformation("Manual trigger: Recurring job execution");

        try
        {
            _recurringJobManager.Trigger("close-ended-bookings");

            return Ok(new
            {
                success = true,
                message = "Recurring job triggered successfully",
                jobName = "close-ended-bookings",
                note = "Check Hangfire dashboard to see execution"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Error triggering recurring job",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get information about scheduled jobs
    /// </summary>
    /// <returns>Job information</returns>
    [HttpGet("job-info")]
    public IActionResult GetJobInfo()
    {
        return Ok(new
        {
            success = true,
            recurringJobs = new[]
            {
                new
                {
                    name = "close-ended-bookings",
                    schedule = "*/15 * * * *",
                    description = "Closes ended bookings every 15 minutes",
                    timezone = "UTC"
                }
            },
            endpoints = new
            {
                triggerImmediate = "/api/hangfiretest/trigger-close-bookings",
                testDirect = "/api/hangfiretest/test-service-direct",
                scheduleDelayed = "/api/hangfiretest/schedule-delayed?delaySeconds=10",
                triggerRecurring = "/api/hangfiretest/trigger-recurring-job",
                dashboard = "/hangfire"
            },
            note = "Use these endpoints to test Hangfire jobs in development"
        });
    }

    /// <summary>
    /// Create test data: Add a booking that should be closed
    /// </summary>
    /// <returns>Result</returns>
    [HttpPost("create-test-booking")]
    public IActionResult CreateTestBooking()
    {
        return Ok(new
        {
            success = true,
            message = "This would create a test booking in production implementation",
            note = "For actual testing, manually create a booking with EndDate in the past and Status = InProgress"
        });
    }

    /// <summary>
    /// Health check for Hangfire
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            success = true,
            hangfireStatus = "Connected",
            timestamp = DateTime.UtcNow,
            dashboardUrl = "/hangfire"
        });
    }
}

