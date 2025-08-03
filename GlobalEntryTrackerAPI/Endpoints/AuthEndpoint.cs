using Business;
using Business.Dto;
using Business.Dto.Requests;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Models;
using Microsoft.AspNetCore.Identity.Data;

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
                async (IAuthBusiness authBusiness, PasswordResetEmailRequest request) =>
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


        // // POST: api/auth/v1/refresh - Refresh Token
        // app.MapPost("/api/auth/v1/refresh-token",
        //         async (IAuthBusiness authBusiness,
        //             HttpResponse response, HttpRequest request) =>
        //         {
        //             var refreshToken = request.Cookies[AuthCookie.RefreshTokenName];
        //             if (string.IsNullOrEmpty(refreshToken))
        //                 return Results.BadRequest();
        //             var token = await authBusiness.RefreshToken(refreshToken);
        //             AuthUtil.SetResponseAuthCookies(response, token);
        //             return Results.Ok(token.AccessToken);
        //         })
        //     .WithTags("Authentication")
        //     .WithName("RefreshToken")
        //     .WithSummary("Refresh an access token using a refresh token")
        //     .WithDescription(
        //         "Uses the provided refresh token to obtain a new access token and refresh token.")
        //     .Produces<ApiResponse<string>>()
        //     .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        // app.MapPost("/api/auth/v1/sign-out",
        //         (HttpResponse response, IConfiguration configuration) =>
        //         {
        //             var domain = configuration["Auth:Cookie_Domain"];
        //             if (string.IsNullOrEmpty(domain))
        //                 return Results.BadRequest("Domain for cookies is not configured.");
        //             AuthUtil.ClearResponseAuthCookies(response, domain);
        //             return Results.Ok();
        //         })
        //     .Produces<ApiResponse<object>>()
        //     .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        // app.MapPost("/api/auth/v1/verify-email",
        //         async (VerifyOtpRequest request, IAuthBusiness authBusiness,
        //             HttpResponse response) =>
        //         {
        //             var tokens =
        //                 await authBusiness.VerifyOtp("signup", request);
        //             AuthUtil.SetResponseAuthCookies(response, tokens);
        //             return Results.Ok();
        //         })
        //     .WithTags("Authentication")
        //     .WithName("VerifyEmail")
        //     .WithSummary("Verify email")
        //     .WithDescription(
        //         "Verifies the provided email when signing up and allows the user to log in.")
        //     .Accepts<VerifyOtpRequest>("application/json")
        //     .Produces<ApiResponse<object>>()
        //     .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        //POST resend-email-verification
        app.MapPost("/api/auth/v1/resend-email-verification",
                async (ResendEmailVerificationRequest request, IAuthBusiness authBusiness) =>
                {
                    await authBusiness.ResendVerificationEmail(request);
                    return Results.Ok();
                })
            .WithTags("Authentication")
            .WithName("ResendEmailVerification")
            .WithSummary("Resend email verification")
            .WithDescription(
                "Resends the email verification link to the user with the provided email address.")
            .Accepts<ResendEmailVerificationRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        // app.MapPost("/api/auth/v1/verify-email-reset",
        //         async (VerifyOtpRequest request, IAuthBusiness authBusiness,
        //             HttpResponse response) =>
        //         {
        //             var tokens =
        //                 await authBusiness.VerifyOtp("recovery", request);
        //             AuthUtil.SetResponseAuthCookies(response, tokens);
        //             return Results.Ok();
        //         })
        //     .WithTags("Authentication")
        //     .WithName("VerifyEmailReset")
        //     .WithSummary("Verify email reset token")
        //     .WithDescription(
        //         "Verifies the provided email reset token and allows the user to reset their password.")
        //     .Accepts<VerifyOtpRequest>("application/json")
        //     .Produces<ApiResponse<object>>()
        //     .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        app.MapPost("/api/auth/v1/reset-password",
                async (HttpContext httpContext, ResetPasswordRequest request,
                    IAuthBusiness authBusiness) =>
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