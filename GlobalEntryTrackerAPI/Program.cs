using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Business;
using Business.Mappers;
using Database;
using Database.Entities;
using Database.Repositories;
using GlobalEntryTrackerAPI.Endpoints;
using GlobalEntryTrackerAPI.Endpoints.Notifications;
using GlobalEntryTrackerAPI.Endpoints.Webhooks;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Jobs;
using GlobalEntryTrackerAPI.Middleware;
using GlobalEntryTrackerAPI.Util;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Quartz;
using Quartz.AspNetCore;
using Serilog;
using Service;
using Service.Jobs;
using Service.Notification;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    var logConfig = configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        );

    if (context.HostingEnvironment.IsDevelopment())
    {
        logConfig.WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        );
    }
});

const string globalEntryTrackerPolicy = "GlobalEntryTrackerPolicy";
var allowedOriginsConfig = builder.Configuration.GetValue<string>("Allowed_Origins");
if (string.IsNullOrWhiteSpace(allowedOriginsConfig))
    throw new Exception("Allowed_Origins configuration is required and cannot be empty.");

var allowedOrigins = allowedOriginsConfig
    .Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
if (allowedOrigins.Length == 0)
    throw new Exception("At least one allowed origin must be configured in Allowed_Origins.");

StripeConfiguration.ApiKey = builder.Configuration.GetValue<string>("Stripe:Secret_Key") ??
                             throw new Exception("Stripe Secret Key is missing.");

builder.Services.AddCors(options =>
{
    options.AddPolicy(globalEntryTrackerPolicy,
        policy =>
        {
            policy.AllowAnyMethod();
            policy.AllowAnyHeader();
            policy.WithOrigins(allowedOrigins);
            policy.AllowCredentials();
        });
});

var dbUsername = builder.Configuration.GetValue<string>("Database:Username");
var dbPassword = builder.Configuration.GetValue<string>("Database:Password");
var dbServer = builder.Configuration.GetValue<string>("Database:Server");
var dbPort = builder.Configuration.GetValue<int?>("Database:Port");
var dbName = builder.Configuration.GetValue<string>("Database:Name");
if (string.IsNullOrEmpty(dbUsername) || string.IsNullOrEmpty(dbPassword) ||
    string.IsNullOrEmpty(dbServer) || string.IsNullOrEmpty(dbName) || dbPort == null)
    throw new Exception("Application Database configuration is missing.");
var connectionString =
    $"Host={dbServer};Port={dbPort};Database={dbName};Username={dbUsername};Password={dbPassword};";

builder.Services.AddDbContextFactory<GlobalEntryTrackerDbContext>(opt =>
{
    opt.UseNpgsql(connectionString);
#if DEBUG
    opt.EnableSensitiveDataLogging();
#endif
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<AppointmentLocationRepository>();
builder.Services.AddScoped<TrackedLocationForUserRepository>();
builder.Services.AddScoped<NotificationTypeRepository>();
builder.Services.AddScoped<UserProfileRepository>();
builder.Services.AddScoped<UserRoleRepository>();
builder.Services.AddScoped<UserCustomerRepository>();
builder.Services.AddScoped<StripeWebhookEventRepository>();
builder.Services.AddScoped<PlanOptionRepository>();
builder.Services.AddScoped<UserNotificationRepository>();
builder.Services.AddScoped<ArchivedAppointmentsRepository>();
builder.Services.AddScoped<RoleRepository>();


//builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AppointmentLocationBusiness>();
builder.Services.AddScoped<UserAppointmentTrackerBusiness>();
builder.Services.AddScoped<NotificationBusiness>();
builder.Services.AddScoped<UserBusiness>();
builder.Services.AddScoped<DiscordNotificationSettingsBusiness>();
builder.Services
    .AddScoped<IEmailNotificationSettingsBusiness, NotAvailableEmailNotificationBusiness>();
builder.Services.AddScoped<NotificationManagerService>();
builder.Services.AddScoped<NotificationDispatcherService>();
builder.Services.AddScoped<SubscriptionBusiness>();
builder.Services.AddScoped<PlanBusiness>();
builder.Services.AddScoped<IAuthBusiness, IdentityAuthBusiness>();
builder.Services.AddScoped<RoleBusiness>();

builder.Services.AddScoped<UserAppointmentValidationService>();
builder.Services.AddScoped<DiscordNotificationService>();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<UserRoleService>();
builder.Services.AddScoped<AppointmentArchiveService>();

builder.Services.AddTransient<SmtpClient>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var smtpHost = config.GetValue<string>("Smtp:Host");
    var smtpPort = config.GetValue<int?>("Smtp:Port");
    var smtpUsername = config.GetValue<string>("Smtp:Username");
    var smtpPassword = config.GetValue<string>("Smtp:Password");
    return new SmtpClient
    {
        UseDefaultCredentials = false,
        EnableSsl = true,
        Host = smtpHost ?? throw new Exception("Smtp Host is missing."),
        Port = smtpPort ?? throw new Exception("Smtp Port is missing."),
        Credentials =
            new NetworkCredential(smtpUsername ?? throw new Exception("Smtp Username missing"),
                smtpPassword ?? throw new Exception("Smtp Password missing")),
        Timeout = 10000
    };
});

builder.Services.AddTransient<IEmailSender<UserEntity>, AwsEmailSender>();

builder.Services.AddHttpClient();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});


//Mappers
builder.Services.AddAutoMapper(typeof(UserMapper).Assembly);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Global Entry Tracker API",
        Version = "v1",
        Description = "API for Global Entry Tracker"
    });
    options.UseInlineDefinitionsForEnums();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});


builder.Services.AddQuartz(q =>
{
    // Use a dedicated thread pool for Quartz jobs.
    q.UseDedicatedThreadPool(tp => { tp.MaxConcurrency = 10; });
    // // Configure Quartz options (optional)

    q.UseInMemoryStore();


    var jobKey = new JobKey("ActiveJobManagerJob");
    q.AddJob<ActiveJobManagerJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ActiveJob-trigger").StartNow().WithSimpleSchedule(s =>
            s.WithIntervalInHours(24)
                .RepeatForever()));

    var syncJobKey = new JobKey("SyncSubscriptionRolesJob");
    q.AddJob<SyncSubscriptionRolesJob>(opts => opts.WithIdentity(syncJobKey));
    q.AddTrigger(opts => opts
        .ForJob(syncJobKey)
        .WithIdentity("SyncSubscriptionRoles-trigger").StartNow().WithSimpleSchedule(s =>
            s.WithIntervalInHours(24)
                .RepeatForever()));
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"))
    .AddPolicy("PaidOrAdmin",
        policy => policy.RequireRole("Subscriber", "Admin", "FriendsFamily"));

builder.Services.AddQuartzServer(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});
builder.Services.ConfigureApplicationCookie(options =>
{
    var cookieDomain = builder.Configuration.GetValue<string>("Auth:Cookie_Domain");
    var isLocalhostCookieDomain = string.IsNullOrWhiteSpace(cookieDomain) ||
                                  cookieDomain.Equals("localhost",
                                      StringComparison.OrdinalIgnoreCase) ||
                                  cookieDomain.Equals("127.0.0.1") ||
                                  cookieDomain.Equals("::1");

    if (!isLocalhostCookieDomain) options.Cookie.Domain = cookieDomain;

    options.Cookie.SameSite = isLocalhostCookieDomain ? SameSiteMode.Lax : SameSiteMode.None;
    options.Cookie.SecurePolicy = isLocalhostCookieDomain
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});
builder.Services.AddIdentityApiEndpoints<UserEntity>(op =>
    {
        op.SignIn.RequireConfirmedEmail = false;
    })
    .AddRoles<RoleEntity>()
    .AddEntityFrameworkStores<GlobalEntryTrackerDbContext>()
    .AddDefaultTokenProviders();
var app = builder.Build();

app.UseSerilogRequestLogging();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.UseSwagger();

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

app.UseCors(globalEntryTrackerPolicy);

app.UseMiddleware<ApiResponseMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapLocationEndpoints();
app.MapLocationTrackerEndpoints();
app.MapSubscriptionEndpoints();
app.MapNotificationEndpoints();
app.MapAuthEndpoints();
app.MapNotificationSettingsEndpoints();
app.MapUserEndpoints();
app.MapStripeWebHooks();
app.MapAdminEndpoints();
app.MapEntryAlertIdentityApi<UserEntity>();

// Notification endpoints
app.MapDiscordNotificationEndpoints();
app.MapEmailNotificationEndpoints();


app.Run();