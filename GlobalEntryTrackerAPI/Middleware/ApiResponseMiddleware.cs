using System.Text.Json;
using GlobalEntryTrackerAPI.Models;

public class ApiResponseMiddleware
{
    private readonly RequestDelegate _next;

    public ApiResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
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
            await _next(context);

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
                    var x = new ApiResponse<object>();
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
            context.Response.Body = originalBodyStream;
            var errorResponse = new ApiResponse<string>(ex.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}