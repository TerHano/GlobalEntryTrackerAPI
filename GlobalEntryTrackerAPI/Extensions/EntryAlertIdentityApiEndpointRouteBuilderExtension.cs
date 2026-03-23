// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Business;
using Business.Dto.Requests;
using Business.Exceptions;
using Database.Entities;
using Database.Enums;
using GlobalEntryTrackerAPI.Models;
using GlobalEntryTrackerAPI.Models.Requests;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using ResetPasswordRequest = Microsoft.AspNetCore.Identity.Data.ResetPasswordRequest;

namespace GlobalEntryTrackerAPI.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IEndpointRouteBuilder" /> to add identity endpoints.
/// </summary>
public static class EntryAlertIdentityApiEndpointRouteBuilderExtension
{
    // Validate the email address using DataAnnotations like the UserValidator does when RequireUniqueEmail = true.
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    /// <summary>
    ///     Add endpoints for registering, logging in, and logging out using ASP.NET Core Identity.
    /// </summary>
    /// <typeparam name="TUser">
    ///     The type describing the user. This should match the generic parameter in
    ///     <see cref="UserManager{TUser}" />.
    /// </typeparam>
    /// <param name="endpoints">
    ///     The <see cref="IEndpointRouteBuilder" /> to add the identity endpoints to.
    ///     Call <see cref="EndpointRouteBuilderExtensions.MapGroup(IEndpointRouteBuilder, string)" /> to add a prefix to all
    ///     the endpoints.
    /// </param>
    /// <returns>An <see cref="IEndpointConventionBuilder" /> to further customize the added endpoints.</returns>
    public static IEndpointConventionBuilder MapEntryAlertIdentityApi<TUser>(
        this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var timeProvider = endpoints.ServiceProvider.GetRequiredService<TimeProvider>();
        var bearerTokenOptions = endpoints.ServiceProvider
            .GetRequiredService<IOptionsMonitor<BearerTokenOptions>>();

        var routeGroup = endpoints.MapGroup("/api/auth/v1/");

        // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        routeGroup.MapPost("/register", async Task<Results<Ok, ValidationProblem>>
            ([FromBody] CreateUserRequest request, HttpContext context,
                SignInManager<TUser> signInManager,
                IAuthBusiness authBusiness,
                IConfiguration configuration,
                [FromServices] IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<TUser>>();

                if (!userManager.SupportsUserEmail)
                    throw new NotSupportedException(
                        $"{nameof(MapEntryAlertIdentityApi)} requires a user store with email support.");

                var userStore = sp.GetRequiredService<IUserStore<TUser>>();
                var emailStore = (IUserEmailStore<TUser>)userStore;
                var email = request.Email;

                if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
                    return CreateValidationProblem(
                        IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));

                var user = new TUser();
                await userStore.SetUserNameAsync(user, email, CancellationToken.None);
                await emailStore.SetEmailAsync(user, email, CancellationToken.None);
                var result = await userManager.CreateAsync(user, request.Password);
                var userId = await userManager.GetUserIdAsync(user);

                if (!result.Succeeded) return CreateValidationProblem(result);
                // Add claims to the user

                await userManager.AddToRoleAsync(user, nameof(Role.Free));
                await authBusiness.CreateUser(request, userId);
                await SendConfirmationEmailAsync(user, userManager, context, email);
                // Note: Not signing in automatically since email confirmation is required
                // User must confirm their email first before they can log in
                return TypedResults.Ok();
            }).WithTags("Authentication")
            .WithName("SignUp")
            .WithSummary("Register a new user")
            .WithDescription(
                "Creates a new user account with the provided registration details. A confirmation email has been sent. Please check your inbox and confirm your email before logging in.")
            .Accepts<CreateUserRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
        ;

        routeGroup.MapPost("/login",
                async Task<Results<Ok<AccessTokenResponse>, Ok, ProblemHttpResult>>
                ([FromBody] LoginRequest login, [FromServices] IServiceProvider sp,
                    IConfiguration configuration) =>
                {
                    var useCookies = true;
                    var useSessionCookies = false;
                    var signInManager = sp.GetRequiredService<SignInManager<TUser>>();

                    var useCookieScheme = useCookies || useSessionCookies;
                    var isPersistent = useCookies && !useSessionCookies;
                    signInManager.AuthenticationScheme = useCookieScheme
                        ? IdentityConstants.ApplicationScheme
                        : IdentityConstants.BearerScheme;

                    var result = await signInManager.PasswordSignInAsync(login.Email,
                        login.Password,
                        isPersistent, true);

                    if (result.RequiresTwoFactor)
                    {
                        if (!string.IsNullOrEmpty(login.TwoFactorCode))
                            result = await signInManager.TwoFactorAuthenticatorSignInAsync(
                                login.TwoFactorCode, isPersistent, isPersistent);
                        else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                            result = await signInManager.TwoFactorRecoveryCodeSignInAsync(
                                login.TwoFactorRecoveryCode);
                    }

                    if (!result.Succeeded)
                    {
                        if (result.IsNotAllowed)
                            throw new EmailNotConfirmedException(
                                "Please confirm your email before logging in. Check your inbox for the confirmation email.");

                        throw new IncorrectLoginInformationException("Wrong email or password.");
                    }

                    // return TypedResults.Problem(result.ToString(),
                    //     statusCode: StatusCodes.Status401Unauthorized);
                    // The signInManager already produced the needed response in the form of a cookie or bearer token.
                    return TypedResults.Ok();
                })
            .WithTags("Authentication")
            .WithName("SignIn")
            .WithSummary("Authenticate a user and obtain a token")
            .WithDescription(
                "Authenticates the user with the provided credentials and returns a token cookie.")
            .Accepts<SignInRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);


        routeGroup.MapPost("/logout", async (SignInManager<UserEntity> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Ok();
            })
            .WithOpenApi()
            .RequireAuthorization();

        routeGroup.MapPost("/refresh",
            async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult,
                    ChallengeHttpResult>>
                ([FromBody] RefreshRequest refreshRequest, [FromServices] IServiceProvider sp) =>
            {
                var signInManager = sp.GetRequiredService<SignInManager<TUser>>();
                var refreshTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme)
                    .RefreshTokenProtector;
                var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

                // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
                if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
                    timeProvider.GetUtcNow() >= expiresUtc ||
                    await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not
                        TUser user)
                    return TypedResults.Challenge();

                var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
                return TypedResults.SignIn(newPrincipal,
                    authenticationScheme: IdentityConstants.BearerScheme);
            });

        routeGroup.MapGet("/confirmEmail",
            async Task<Results<Ok, UnauthorizedHttpResult>>
            ([FromQuery] string userId, [FromQuery] string code,
                [FromQuery] string? changedEmail, [FromServices] IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<TUser>>();
                if (await userManager.FindByIdAsync(userId) is not { } user)
                    return TypedResults.Unauthorized();

                try
                {
                    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                }
                catch (FormatException)
                {
                    return TypedResults.Unauthorized();
                }

                IdentityResult result;

                if (string.IsNullOrEmpty(changedEmail))
                {
                    result = await userManager.ConfirmEmailAsync(user, code);
                }
                else
                {
                    result = await userManager.ChangeEmailAsync(user, changedEmail, code);

                    if (result.Succeeded)
                        result = await userManager.SetUserNameAsync(user, changedEmail);
                }

                if (!result.Succeeded) return TypedResults.Unauthorized();

                // Sign in the user and set auth cookie
                var signInManager = sp.GetRequiredService<SignInManager<TUser>>();
                await signInManager.SignInAsync(user, true);

                return TypedResults.Ok();
            });

        routeGroup.MapPost("/verify-email",
            async Task<IResult> (
                ConfirmEmailRequest request,
                [FromServices] IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<TUser>>();
                if (await userManager.FindByIdAsync(request.UserId) is not { } user)
                    // We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
                    return TypedResults.Unauthorized();
                string code;
                try
                {
                    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
                }
                catch (FormatException)
                {
                    return TypedResults.Unauthorized();
                }

                IdentityResult result;

                if (string.IsNullOrEmpty(request.ChangedEmail))
                {
                    result = await userManager.ConfirmEmailAsync(user, code);
                }
                else
                {
                    // As with Identity UI, email and user name are one and the same. So when we update the email,
                    // we need to update the user name.
                    result = await userManager.ChangeEmailAsync(user, request.ChangedEmail, code);

                    if (result.Succeeded)
                        result = await userManager.SetUserNameAsync(user, request.ChangedEmail);
                }

                if (!result.Succeeded) throw new Exception(result.Errors.First().Description);

                return Results.Ok();
            });

        routeGroup.MapPost("/resendConfirmationEmail", async Task<Ok>
        ([FromBody] ResendConfirmationEmailRequest resendRequest, HttpContext context,
            [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            if (await userManager.FindByEmailAsync(resendRequest.Email) is not { } user)
                return TypedResults.Ok();

            await SendConfirmationEmailAsync(user, userManager, context, resendRequest.Email);
            return TypedResults.Ok();
        });

        routeGroup.MapPost("/forgotPassword", async Task<Results<Ok, ValidationProblem>>
            ([FromBody] ForgotPasswordRequest resetRequest,
                [FromServices] IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<TUser>>();
                var user = await userManager.FindByEmailAsync(resetRequest.Email);

                if (user is not null && await userManager.IsEmailConfirmedAsync(user))
                {
                    var code = await userManager.GeneratePasswordResetTokenAsync(user);
                    //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var emailSender = sp.GetRequiredService<IEmailSender<TUser>>();

                    var frontendBaseUrl = sp.GetRequiredService<IConfiguration>()
                                              .GetValue<string>("Frontend:Base_Url")
                                          ?? throw new NotSupportedException(
                                              "Frontend:Base_Url is not configured.");
                    var frontEndResetPasswordEndpoint =
                        sp.GetRequiredService<IConfiguration>()
                            .GetValue("Frontend:Reset_Password_Endpoint", "/reset-password");
                    var resetPasswordUrl = QueryHelpers.AddQueryString(
                        $"{frontendBaseUrl}{frontEndResetPasswordEndpoint}",
                        new Dictionary<string, string?>
                        {
                            ["email"] = resetRequest.Email,
                            ["code"] = code
                        });

                    await emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email,
                        code);
                }

                // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
                // returned a 400 for an invalid code given a valid user email.
                return TypedResults.Ok();
            })
            .WithTags("Authentication")
            .WithName("ForgotPassword")
            .WithSummary("Initiate a password reset request")
            .WithDescription(
                "Sends a password reset email to the user with the provided email address if it exists and is confirmed. The email contains a reset code and a link to reset the password. To reset the password, make a /resetPassword request with the email, reset code, and new password.")
            .Accepts<ForgotPasswordRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        routeGroup.MapPost("/resetPassword", async Task<Results<Ok, ValidationProblem>>
            ([FromBody] ResetPasswordRequest resetRequest,
                [FromServices] IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<TUser>>();

                var user = await userManager.FindByEmailAsync(resetRequest.Email);

                if (user is null || !await userManager.IsEmailConfirmedAsync(user))
                    // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
                    // returned a 400 for an invalid code given a valid user email.
                    return CreateValidationProblem(
                        IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));

                IdentityResult result;
                try
                {
                    var code =
                        Encoding.UTF8.GetString(
                            WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
                    result = await userManager.ResetPasswordAsync(user, code,
                        resetRequest.NewPassword);
                }
                catch (FormatException)
                {
                    result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
                }

                if (!result.Succeeded) return CreateValidationProblem(result);

                return TypedResults.Ok();
            })
            .WithTags("Authentication")
            .WithName("ResetPassword")
            .WithSummary("Reset a user's password")
            .WithDescription(
                "Resets the user's password using the provided reset code. To get a reset code, first make a /forgotPassword request with the user's email to receive the reset code in an email.")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);


        var accountGroup = routeGroup.MapGroup("/manage").RequireAuthorization();

        accountGroup.MapPost("/2fa",
            async Task<Results<Ok<TwoFactorResponse>, ValidationProblem, NotFound>>
            (ClaimsPrincipal claimsPrincipal, [FromBody] TwoFactorRequest tfaRequest,
                [FromServices] IServiceProvider sp) =>
            {
                var signInManager = sp.GetRequiredService<SignInManager<TUser>>();
                var userManager = signInManager.UserManager;
                if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
                    return TypedResults.NotFound();

                if (tfaRequest.Enable == true)
                {
                    if (tfaRequest.ResetSharedKey)
                        return CreateValidationProblem("CannotResetSharedKeyAndEnable",
                            "Resetting the 2fa shared key must disable 2fa until a 2fa token based on the new shared key is validated.");

                    if (string.IsNullOrEmpty(tfaRequest.TwoFactorCode))
                        return CreateValidationProblem("RequiresTwoFactor",
                            "No 2fa token was provided by the request. A valid 2fa token is required to enable 2fa.");

                    if (!await userManager.VerifyTwoFactorTokenAsync(user,
                            userManager.Options.Tokens.AuthenticatorTokenProvider,
                            tfaRequest.TwoFactorCode))
                        return CreateValidationProblem("InvalidTwoFactorCode",
                            "The 2fa token provided by the request was invalid. A valid 2fa token is required to enable 2fa.");

                    await userManager.SetTwoFactorEnabledAsync(user, true);
                }
                else if (tfaRequest.Enable == false || tfaRequest.ResetSharedKey)
                {
                    await userManager.SetTwoFactorEnabledAsync(user, false);
                }

                if (tfaRequest.ResetSharedKey) await userManager.ResetAuthenticatorKeyAsync(user);

                string[]? recoveryCodes = null;
                if (tfaRequest.ResetRecoveryCodes || (tfaRequest.Enable == true &&
                                                      await userManager.CountRecoveryCodesAsync(
                                                          user) == 0))
                {
                    var recoveryCodesEnumerable =
                        await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                    recoveryCodes = recoveryCodesEnumerable?.ToArray();
                }

                if (tfaRequest.ForgetMachine) await signInManager.ForgetTwoFactorClientAsync();

                var key = await userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrEmpty(key))
                {
                    await userManager.ResetAuthenticatorKeyAsync(user);
                    key = await userManager.GetAuthenticatorKeyAsync(user);

                    if (string.IsNullOrEmpty(key))
                        throw new NotSupportedException(
                            "The user manager must produce an authenticator key after reset.");
                }

                return TypedResults.Ok(new TwoFactorResponse
                {
                    SharedKey = key,
                    RecoveryCodes = recoveryCodes,
                    RecoveryCodesLeft = recoveryCodes?.Length ??
                                        await userManager.CountRecoveryCodesAsync(user),
                    IsTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
                    IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user)
                });
            });

        accountGroup.MapGet("/info",
            async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<TUser>>();
                if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
                    return TypedResults.NotFound();

                return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
            });

        accountGroup.MapPost("/info",
            async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>>
            (ClaimsPrincipal claimsPrincipal, [FromBody] InfoRequest infoRequest,
                HttpContext context, [FromServices] IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<TUser>>();
                if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
                    return TypedResults.NotFound();

                if (!string.IsNullOrEmpty(infoRequest.NewEmail) &&
                    !_emailAddressAttribute.IsValid(infoRequest.NewEmail))
                    return CreateValidationProblem(IdentityResult.Failed(
                        userManager.ErrorDescriber.InvalidEmail(infoRequest.NewEmail)));

                if (!string.IsNullOrEmpty(infoRequest.NewPassword))
                {
                    if (string.IsNullOrEmpty(infoRequest.OldPassword))
                        return CreateValidationProblem("OldPasswordRequired",
                            "The old password is required to set a new password. If the old password is forgotten, use /resetPassword.");

                    var changePasswordResult = await userManager.ChangePasswordAsync(user,
                        infoRequest.OldPassword, infoRequest.NewPassword);
                    if (!changePasswordResult.Succeeded)
                        return CreateValidationProblem(changePasswordResult);
                }

                if (!string.IsNullOrEmpty(infoRequest.NewEmail))
                {
                    var email = await userManager.GetEmailAsync(user);

                    if (email != infoRequest.NewEmail)
                        await SendConfirmationEmailAsync(user, userManager, context,
                            infoRequest.NewEmail, true);
                }

                return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
            });

        async Task SendConfirmationEmailAsync(TUser user, UserManager<TUser> userManager,
            HttpContext context, string email, bool isChange = false)
        {
            var code = isChange
                ? await userManager.GenerateChangeEmailTokenAsync(user, email)
                : await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var userId = await userManager.GetUserIdAsync(user);

            var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
            var frontendBaseUrl = configuration.GetValue<string>("Frontend:Base_Url")
                                  ?? throw new NotSupportedException(
                                      "Frontend:Base_Url is not configured.");
            var frontEndConfirmEmailEndpoint =
                configuration.GetValue("Frontend:Confirm_Email_Endpoint", "/confirm-email");

            var queryParams = new Dictionary<string, string?>
            {
                ["userId"] = userId,
                ["code"] = code,
                ["email"] = email
            };
            if (isChange)
                queryParams["changedEmail"] = email;

            var confirmEmailUrl =
                QueryHelpers.AddQueryString($"{frontendBaseUrl}{frontEndConfirmEmailEndpoint}",
                    queryParams);

            var emailSender = context.RequestServices.GetRequiredService<IEmailSender<TUser>>();
            await emailSender.SendConfirmationLinkAsync(user, email, confirmEmailUrl);
        }

        return new IdentityEndpointsConventionBuilder(routeGroup);
    }

    private static ValidationProblem CreateValidationProblem(string errorCode,
        string errorDescription)
    {
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            { errorCode, [errorDescription] }
        });
    }

    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        // We expect a single error code and description in the normal case.
        // This could be golfed with GroupBy and ToDictionary, but perf! :P
        Debug.Assert(!result.Succeeded);
        var errorDictionary = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out var descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return TypedResults.ValidationProblem(errorDictionary);
    }

    private static async Task<InfoResponse> CreateInfoResponseAsync<TUser>(TUser user,
        UserManager<TUser> userManager)
        where TUser : class
    {
        return new InfoResponse
        {
            Email = await userManager.GetEmailAsync(user) ??
                    throw new NotSupportedException("Users must have an email."),
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user)
        };
    }

    // Wrap RouteGroupBuilder with a non-public type to avoid a potential future behavioral breaking change.
    private sealed class IdentityEndpointsConventionBuilder(RouteGroupBuilder inner)
        : IEndpointConventionBuilder
    {
        private IEndpointConventionBuilder InnerAsConventionBuilder => inner;

        public void Add(Action<EndpointBuilder> convention)
        {
            InnerAsConventionBuilder.Add(convention);
        }

        public void Finally(Action<EndpointBuilder> finallyConvention)
        {
            InnerAsConventionBuilder.Finally(finallyConvention);
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromBodyAttribute : Attribute, IFromBodyMetadata
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromServicesAttribute : Attribute, IFromServiceMetadata
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromQueryAttribute : Attribute, IFromQueryMetadata
    {
        public string? Name => null;
    }
}