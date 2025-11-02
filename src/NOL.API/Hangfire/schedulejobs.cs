using Hangfire;
using NOL.Application.Common.Interfaces;

namespace NOL.Application.Hangfire;

public static  class schedulejobsExtensions
{
    public static void ScheduleJobs(this IApplicationBuilder app)
    {
        
        var recurringJobs = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

     
        recurringJobs.AddOrUpdate<EndBookingSchedulJob>(
            "test-job",                             
            job => job.Execute(),                    
            "* * * * *"                         
        );
    }
}