using System.Net;
using System.Text.Json;
using Business.Enum;
using Business.Exceptions;
using FluentValidation;
using GlobalEntryTrackerAPI.Models;

namespace GlobalEntryTrackerAPI.Middleware;

public class ApiResponseMiddleware(RequestDelegate next, ILogger<ApiResponseMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
            // If the response has already started, skip further processing
            return;
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        try
        {
            await next(context);

            context.Response.Body = originalBodyStream;
            if (context.Response.StatusCode is >= 200 and < 300)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var bodyText = await new StreamReader(responseBody).ReadToEndAsync();

                ApiResponse<object?> wrappedResponse;

                string wrappedResponseJson;
                if (string.IsNullOrWhiteSpace(bodyText))
                {
                    // Handle empty or null response
                    wrappedResponseJson =
                        JsonSerializer.Serialize(new ApiResponse<object>(), options);
                }
                else
                {
                    // Handle non-empty response
                    var responseData = JsonSerializer.Deserialize<object?>(bodyText);
                    wrappedResponseJson =
                        JsonSerializer.Serialize(new ApiResponse<object?>(responseData), options);
                }

                context.Response.ContentType = "application/json";
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
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
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

                var responseJson = JsonSerializer.Serialize(response, options);
                logger.LogError(ex, "An error occurred while processing the request.");
                await context.Response.WriteAsync(responseJson);
            }
        }
    }
}