using System.Net;
using System.Text.Json;
using Business.Enum;
using Business.Exceptions;
using FluentValidation;
using GlobalEntryTrackerAPI.Models;

namespace GlobalEntryTrackerAPI.Middleware;

public class ApiResponseMiddleware(RequestDelegate next, ILogger<ApiResponseMiddleware> logger)
{
    // Static so the serializer's per-options metadata cache is actually reused; a fresh instance
    // per request defeats it. Not indented: this is wire format, not something a human reads.
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
            // If the response has already started, skip further processing
            return;
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);

            context.Response.Body = originalBodyStream;
            if (context.Response.StatusCode is >= 200 and < 300)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var bodyText = await new StreamReader(responseBody).ReadToEndAsync();

                // Only bypass wrapping if content-type is explicitly set to something non-JSON
                // (e.g. text/plain, image/*, etc.). Empty content-type (e.g. Ok() with no body) should still be wrapped.
                var contentType = context.Response.ContentType ?? string.Empty;
                if (!string.IsNullOrEmpty(contentType) && !contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                    return;
                }

                string wrappedResponseJson;
                if (string.IsNullOrWhiteSpace(bodyText))
                {
                    wrappedResponseJson =
                        JsonSerializer.Serialize(new ApiResponse<object>(), SerializerOptions);
                }
                else
                {
                    var responseData = JsonSerializer.Deserialize<object?>(bodyText);
                    wrappedResponseJson =
                        JsonSerializer.Serialize(new ApiResponse<object?>(responseData), SerializerOptions);
                }

                context.Response.ContentType = "application/json";
                context.Response.Headers.ContentLength = null;
                await context.Response.WriteAsync(wrappedResponseJson);
            }
            else
            {
                // Handle non-successful status codes
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
        catch (Exception ex)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.Body = originalBodyStream;

                ApiResponse<object?> response;
                if (ex is ValidationException validationException)
                {
                    var errors = validationException.Errors
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    response = new ApiResponse<object?>
                    {
                        Success = false,
                        Errors = errors.Select<string, Error>(e =>
                        {
                            return new Error
                            {
                                Code = ExceptionCode.GenericError,
                                Message = e
                            };
                        }).ToList()
                    };
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                else if (ex is BaseApplicationException baseApplicationException)
                {
                    response = new ApiResponse<object?>
                    {
                        Success = false,
                        Errors =
                        [
                            new Error
                            {
                                Code = baseApplicationException.ErrorCode,
                                Message = baseApplicationException.Message
                            }
                        ]
                    };
                    // These describe something the caller did or a state they can resolve, so they
                    // carry their own 4xx rather than being reported as a server fault.
                    context.Response.StatusCode = (int)baseApplicationException.StatusCode;
                }
                else if (ex is UnauthorizedAccessException)
                {
                    response = new ApiResponse<object?>
                    {
                        Success = false,
                        Errors =
                        [
                            new Error
                            {
                                Code = ExceptionCode.GenericError,
                                // Deliberately vague: the caller should not learn whether the
                                // resource exists, only that it is not theirs.
                                Message = "You are not authorized to perform this action."
                            }
                        ]
                    };
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    response = new ApiResponse<object?>
                    {
                        Success = false,
                        Errors =
                        [
                            new Error
                            {
                                Code = ExceptionCode.GenericError,
                                Message = "An unexpected error occurred."
                            }
                        ]
                    };
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

                context.Response.ContentType = "application/json";

                var responseJson = JsonSerializer.Serialize(response, SerializerOptions);
                logger.LogError(ex, "An error occurred while processing the request.");
                await context.Response.WriteAsync(responseJson);
            }
        }
    }
}