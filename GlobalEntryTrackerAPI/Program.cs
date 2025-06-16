using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Business;
using Business.Mappers;
using Database;
using Database.Repositories;
using GlobalEntryTrackerAPI.Endpoints;
using GlobalEntryTrackerAPI.Endpoints.Notifications;
using GlobalEntryTrackerAPI.Enum;
using GlobalEntryTrackerAPI.Middleware;
using GlobalEntryTrackerAPI.Util;
using GlobalEntryTrackerAPI.Webhooks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using Quartz.AspNetCore;
using Service;
using Service.Jobs;
using Service.Notification;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

const string globalEntryTrackerPolicy = "GlobalEntryTrackerPolicy";
var allowedOrigins = builder.Configuration.GetValue<string>("Allowed_Origins") ?? "";

StripeConfiguration.ApiKey = builder.Configuration.GetValue<string>("Stripe:Secret_Key") ??
                             throw new Exception("Stripe Secret Key is missing.");

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
    opt.EnableSensitiveDataLogging();
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<AppointmentLocationRepository>();
builder.Services.AddScoped<TrackedLocationForUserRepository>();
builder.Services.AddScoped<NotificationTypeRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserRoleRepository>();
builder.Services.AddScoped<UserCustomerRepository>();
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
builder.Services.AddScoped<EmailNotificationSettingsBusiness>();
builder.Services.AddScoped<NotificationManagerService>();
builder.Services.AddScoped<NotificationDispatcherService>();
builder.Services.AddScoped<SubscriptionBusiness>();
builder.Services.AddScoped<PlanBusiness>();
builder.Services.AddScoped<AuthBusiness>();
builder.Services.AddScoped<RoleBusiness>();

builder.Services.AddScoped<UserAppointmentValidationService>();
builder.Services.AddScoped<DiscordNotificationService>();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<UserRoleService>();
builder.Services.AddScoped<AppointmentArchiveService>();

builder.Services.AddScoped<SmtpClient>(serviceProvider =>
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
                smtpPassword ?? throw new Exception("Smtp Password missing"))
    };
});

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


var authIssuer = builder.Configuration.GetValue<string>("Auth:Issuer");
var authAudience = builder.Configuration.GetValue<string>("Auth:Audience");
var authSigningKey = builder.Configuration.GetValue<string>("Auth:Signing_Key");
builder.Services.AddAuthentication(options =>
{
    // Identity made Cookie authentication the default.
    // However, we want JWT Bearer Auth to be the default.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
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
            await AuthUtil.OnJWTValidate(context);
        }
    };
});

builder.Services.AddQuartz(q =>
{
    // Use a dedicated thread pool for Quartz jobs.
    q.UseDedicatedThreadPool(tp => { tp.MaxConcurrency = 10; });
    // // Configure Quartz options (optional)

    q.UseInMemoryStore();

    // q.UsePersistentStore(options =>
    // {
    //     
    //     options.UseProperties = false; // Use property-based storage
    //     //use memory store for testing
    //     options.UseClustering(); // Enable clustering if needed
    //     options.UsePostgres(connectionString); // Use SQL Server
    //     options.UseNewtonsoftJsonSerializer();
    // });

    var jobKey = new JobKey("ActiveJobManagerJob");
    q.AddJob<ActiveJobManagerJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ActiveJob-trigger").StartNow().WithSimpleSchedule(s =>
            s.WithIntervalInHours(24)
                .RepeatForever()));
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"));

builder.Services.AddQuartzServer(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});

var app = builder.Build();

// Place this before app.UseAuthentication();
app.Use(async (context, next) =>
{
    var token = context.Request.Cookies[AuthCookie.AccessTokenName];
    if (!string.IsNullOrEmpty(token)) context.Request.Headers.Authorization = $"Bearer {token}";
    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.UseSwagger();

app.UseHttpsRedirection();
//app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors(globalEntryTrackerPolicy);

app.UseMiddleware<ApiResponseMiddleware>();

app.MapLocationEndpoints();
app.MapLocationTrackerEndpoints();
app.MapSubscriptionEndpoints();
app.MapNotificationEndpoints();
app.MapAuthEndpoints();
app.MapNotificationSettingsEndpoints();
app.MapUserEndpoints();
app.MapStripeWebHooks();
app.MapAdminEndpoints();

//Notification endpoints
app.MapDiscordNotificationEndpoints();
app.MapEmailNotificationEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.Run();