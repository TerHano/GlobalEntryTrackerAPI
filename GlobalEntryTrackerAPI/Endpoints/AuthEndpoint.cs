using Business;
using Business.Dto;
using Business.Dto.Requests;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Models;
using GlobalEntryTrackerAPI.Util;
using Supabase.Gotrue;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class AuthEndpoint
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/auth/v1/authenticated",
                (
                    HttpContext httpContext) =>
                {
                    try
                    {
                        httpContext.User.GetUserId();
                    }
                    catch (Exception)
                    {
                        return Results.Ok(new IsAuthenticatedDto
                        {
                            IsAuthenticated = false
                        });
                    }

                    return Results.Ok(new IsAuthenticatedDto
                    {
                        IsAuthenticated = true
                    });
                })
            .WithTags("Authentication")
            .WithName("IsAuthenticated")
            .WithSummary("Check if user is authenticated")
            .WithDescription("Checks if the user is authenticated and returns a boolean value.")
            .Produces<ApiResponse<IsAuthenticatedDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        app.MapPost("/api/auth/v1/password-recovery",
                async (AuthBusiness authBusiness, PasswordResetEmailRequest request) =>
                {
                    await authBusiness.SendPasswordResetEmail(request);
                    return Results.Ok();
                })
            .WithTags("Authentication")
            .WithName("SendPasswordResetEmail")
            .WithSummary("Send password reset email")
            .WithDescription(
                "Sends a password reset email to the user with the provided email address.")
            .Accepts<PasswordResetEmailRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);


        app.MapPost("/api/auth/v1/sign-up",
                async (CreateUserRequest request, AuthBusiness authBusiness) =>
                {
                    await authBusiness.CreateUser(request);
                    return Results.Ok();
                })
            .WithTags("Authentication")
            .WithName("SignUp")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account with the provided registration details.")
            .Accepts<CreateUserRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);


        // POST: api/v1/sign-in Sign In
        app.MapPost("/api/auth/v1/sign-in",
                async (SignInRequest request, AuthBusiness authBusiness, HttpResponse response) =>
                {
                    var token = await authBusiness.SignIn(request);
                    //Append both tokens to the response cookie
                    AuthUtil.SetResponseAuthCookies(response, token);
                    return Results.Ok();
                })
            .WithTags("Authentication")
            .WithName("SignIn")
            .WithSummary("Authenticate a user and obtain a token")
            .WithDescription(
                "Authenticates the user with the provided credentials and returns an access token and refresh token.")
            .Accepts<SignInRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        // POST: api/auth/v1/refresh - Refresh Token
        app.MapPost("/api/auth/v1/refresh-token",
                async (AuthBusiness authBusiness,
                    HttpResponse response, HttpRequest request) =>
                {
                    var refreshToken = request.Cookies["refresh_token"];
                    if (string.IsNullOrEmpty(refreshToken))
                        return Results.BadRequest();
                    var token = await authBusiness.RefreshToken(refreshToken);
                    AuthUtil.SetResponseAuthCookies(response, token);
                    return Results.Ok(token.AccessToken);
                })
            .WithTags("Authentication")
            .WithName("RefreshToken")
            .WithSummary("Refresh an access token using a refresh token")
            .WithDescription(
                "Uses the provided refresh token to obtain a new access token and refresh token.")
            .Produces<ApiResponse<string>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        app.MapPost("/api/auth/v1/sign-out",
                (HttpResponse response, IConfiguration configuration) =>
                {
                    var domain = configuration["Auth:Cookie_Domain"];
                    if (string.IsNullOrEmpty(domain))
                        return Results.BadRequest("Domain for cookies is not configured.");
                    AuthUtil.ClearResponseAuthCookies(response, domain);
                    return Results.Ok();
                })
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        app.MapPost("/api/auth/v1/verify-email",
                async (VerifyOtpRequest request, AuthBusiness authBusiness,
                    HttpResponse response) =>
                {
                    var tokens =
                        await authBusiness.VerifyOtp(Constants.EmailOtpType.Signup, request);
                    AuthUtil.SetResponseAuthCookies(response, tokens);
                    return Results.Ok();
                })
            .WithTags("Authentication")
            .WithName("VerifyEmail")
            .WithSummary("Verify email")
            .WithDescription(
                "Verifies the provided email when signing up and allows the user to log in.")
            .Accepts<VerifyOtpRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        app.MapPost("/api/auth/v1/verify-email-reset",
                async (VerifyOtpRequest request, AuthBusiness authBusiness,
                    HttpResponse response) =>
                {
                    var tokens =
                        await authBusiness.VerifyOtp(Constants.EmailOtpType.Recovery, request);
                    AuthUtil.SetResponseAuthCookies(response, tokens);
                    return Results.Ok();
                })
            .WithTags("Authentication")
            .WithName("VerifyEmailReset")
            .WithSummary("Verify email reset token")
            .WithDescription(
                "Verifies the provided email reset token and allows the user to reset their password.")
            .Accepts<VerifyOtpRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        app.MapPost("/api/auth/v1/reset-password",
                async (HttpContext httpContext, ResetPasswordRequest request,
                    AuthBusiness authBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();
                    await authBusiness.ResetPasswordForUser(userId, request);
                    return Results.Ok();
                })
            .WithTags("Authentication")
            .WithName("ResetPassword")
            .WithSummary("Reset user password")
            .WithDescription(
                "Resets the password for the authenticated user.")
            .Accepts<ResetPasswordRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }
}