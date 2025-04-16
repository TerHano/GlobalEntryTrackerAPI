using System.Net;
using System.Text.Json;
using GlobalEntryTrackerAPI.Models;

namespace GlobalEntryTrackerAPI.Middleware;

public class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (!context.Response.HasStarted)
            {
                await HandleExceptionAsync(context, ex);
            }
            else
            {
                // Log the error if the response has already started
                logger.LogError("Response has already started, unable to handle exception.");
                throw;
            }
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = new ApiResponse<object>(exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var responseJson = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(responseJson);
    }
}