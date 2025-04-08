using Quartz;
using Quartz.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddQuartz(q =>
{
    // Use a dedicated thread pool for Quartz jobs.
    // q.UseDedicatedThreadPool(tp => {
    //     tp.MaxConcurrency = 10; // Adjust as needed
    // });
    // // Configure Quartz options (optional)
    // q.UseMicrosoftDependencyInjectionJobFactory();
    // q.UseInMemoryStore();
    //
    // // Register jobs and triggers here (see step 3)
    // // Example: Register a job and trigger to run every 5 seconds.
    // var jobKey = new JobKey("SampleJob");
    // q.AddJob<SampleJob>(opts => opts.WithIdentity(jobKey));
    // q.AddTrigger(opts => opts
    //     .ForJob(jobKey)
    //     .WithIdentity("SampleJob-trigger")
    //     .WithCronSchedule("0/5 * * * * ?")); // Execute every 5 seconds
});

builder.Services.AddQuartzServer(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/weatherforecast", () =>
{

});

app.Run();
