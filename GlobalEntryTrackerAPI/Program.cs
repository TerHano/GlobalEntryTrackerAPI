using Database;
using Database.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.AspNetCore;
using BusinessLayer;

var builder = WebApplication.CreateBuilder(args);


var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins") ?? "";


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "WerewolfServerPolicy",
        policy =>
        {
            policy.AllowAnyMethod();
            policy.AllowAnyHeader();
            policy.WithOrigins(allowedOrigins);
            policy.AllowCredentials();
        });
});


var connectionString = builder.Configuration.GetValue<string>("Database:ConnectionString");
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("ConnectionString is missing.");
}

builder.Services.AddDbContextPool<GlobalEntryTrackerDbContext>(opt => opt.UseNpgsql(connectionString));

builder.Services.AddScoped<AppointmentLocationRepository>();


//builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AppointmentLocationBusiness>();

// builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

//Mappers
builder.Services.AddAutoMapper(typeof(AppointmentLocationBusiness),typeof(UserAppointmentTrackerBusiness));


//Validators
//builder.Services.AddScoped<IValidator<PlayerDTO>, PlayerDTOValidator>();
//builder.Services.AddScoped<IValidator<UpdateRoleSettingsRequest>, RoleSettingsRequestValidator>();


//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


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


app.MapGet("/weatherforecast", () => { });

app.Run();