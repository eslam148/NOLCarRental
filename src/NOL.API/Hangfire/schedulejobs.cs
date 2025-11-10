using Hangfire;
using NOL.Application.Common.Interfaces;

namespace NOL.Application.Hangfire;

/// <summary>
/// Extension methods for scheduling Hangfire recurring jobs
/// </summary>
public static class ScheduleJobsExtensions
{
    /// <summary>
    /// Configures and schedules all recurring background jobs
    /// </summary>
    public static void ScheduleJobs(this IApplicationBuilder app)
    {
        var recurringJobs = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

        // Close ended bookings - runs every 15 minutes
        // Cron: "*/15 * * * *" = Every 15 minutes
        recurringJobs.AddOrUpdate<EndBookingSchedulJob>(
            "close-ended-bookings",
            job => job.Execute(),
            "* * * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
        
        // Future jobs can be added here:
        // - Send booking reminders (24 hours before)
        // - Process expired loyalty points
        // - Generate daily/weekly reports
        // - Clean up old logs/temp files
    }
}