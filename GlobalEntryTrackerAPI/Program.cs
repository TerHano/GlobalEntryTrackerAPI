using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Business;
using Business.Mappers;
using Database;
using Database.Repositories;
using GlobalEntryTrackerAPI.Endpoints;
using GlobalEntryTrackerAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Quartz.AspNetCore;
using Service;
using Service.Notification;

var builder = WebApplication.CreateBuilder(args);

const string globalEntryTrackerPolicy = "GlobalEntryTrackerPolicy";
var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins") ?? "";


builder.Services.AddCors(options =>
{
    options.AddPolicy(globalEntryTrackerPolicy,
        policy =>
        {
            policy.AllowAnyMethod();
            policy.AllowAnyHeader();
            policy.WithOrigins(allowedOrigins.Split(","));
            policy.AllowCredentials();
        });
});


var connectionString = builder.Configuration.GetValue<string>("Database:ConnectionString");
if (string.IsNullOrEmpty(connectionString)) throw new Exception("ConnectionString is missing.");

builder.Services.AddDbContextPool<GlobalEntryTrackerDbContext>(opt =>
    opt.UseNpgsql(connectionString));


builder.Services.AddScoped<AppointmentLocationRepository>();
builder.Services.AddScoped<TrackedLocationForUserRepository>();
builder.Services.AddScoped<NotificationTypeRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<DiscordNotificationSettingsRepository>();
builder.Services.AddScoped<UserRoleRepository>();


//builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AppointmentLocationBusiness>();
builder.Services.AddScoped<UserAppointmentTrackerBusiness>();
builder.Services.AddScoped<NotificationBusiness>();
builder.Services.AddScoped<UserBusiness>();
builder.Services.AddScoped<DiscordNotificationSettingsBusiness>();
builder.Services.AddScoped<NotificationManagerService>();
builder.Services.AddScoped<NotificationDispatcherService>();

builder.Services.AddScoped<UserAppointmentValidationService>();
builder.Services.AddScoped<DiscordNotificationService>();
builder.Services.AddScoped<JobService>();


builder.Services.AddHttpClient();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});


// builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

//Mappers

builder.Services.AddAutoMapper(typeof(UserMapper).Assembly);


//Validators
//builder.Services.AddScoped<IValidator<PlayerDTO>, PlayerDTOValidator>();
//builder.Services.AddScoped<IValidator<UpdateRoleSettingsRequest>, RoleSettingsRequestValidator>();


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var authIssuer = builder.Configuration.GetValue<string>("Auth:Issuer");
var authAudience = builder.Configuration.GetValue<string>("Auth:Audience");
var authSigningKey = builder.Configuration.GetValue<string>("Auth:SigningKey");
builder.Services.AddAuthentication(options =>
{
    // Identity made Cookie authentication the default.
    // However, we want JWT Bearer Auth to be the default.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    // Configure the Authority to the expected value for
    // the authentication provider. This ensures the token
    // is appropriately validated.
    //options.Authority = "AuthorityURL"; // TODO: Update URL

    // Sending the access token in the query string is required when using WebSockets or ServerSentEvents
    // due to a limitation in Browser APIs. We restrict it to only calls to the
    // SignalR hub in this code.
    // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
    // for more information about security considerations when using
    // the query string to transmit the access token.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = authIssuer,
        //ValidAudience = authAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSigningKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            // Get user ID from the JWT's claims (e.g., "sub" claim)
            var externalUserId =
                context.Principal?.FindFirstValue(ClaimTypes
                    .NameIdentifier); // Or other user identifier claim
            if (string.IsNullOrEmpty(externalUserId))
            {
                context.Fail("User ID not found in claims");
                return;
            }

            await using var dbContext =
                context.HttpContext.RequestServices
                    .GetRequiredService<GlobalEntryTrackerDbContext>();

            var user = await dbContext.Users.FirstOrDefaultAsync(user =>
                user.ExternalId.Equals(externalUserId));

            if (user == null)
            {
                context.Fail("User not found");
                return;
            }

            var claims = new List<Claim>
            {
                new("InternalId", user.Id.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            context.Principal?.AddIdentity(identity);
        }
    };
});

builder.Services.AddQuartz(q =>
{
    // Use a dedicated thread pool for Quartz jobs.
    q.UseDedicatedThreadPool(tp =>
    {
        tp.MaxConcurrency = 10; // Adjust as needed
    });
    // // Configure Quartz options (optional)
    //q.UseMicrosoftDependencyInjectionJobFactory();
    q.UseInMemoryStore();
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
builder.Services.AddAuthorization();

builder.Services.AddQuartzServer(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();
//app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseMiddleware<ApiResponseMiddleware>();

app.MapLocationEndpoints();
app.MapLocationTrackerEndpoints();
app.MapNotificationEndpoints();
app.MapAuthEndpoints();
app.MapNotificationSettingsEndpoints();
app.MapUserEndpoints();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors(globalEntryTrackerPolicy);

app.Run();