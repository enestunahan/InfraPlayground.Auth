using System.Text.Json;
using InfraPlayground.Auth.Application.Common.Exceptions;

namespace InfraPlayground.Auth.API.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AuthenticationFailedException ex)
        {
            logger.LogWarning(ex, "Authentication failed");
            await WriteAsync(context, StatusCodes.Status401Unauthorized, "Authentication Failed", ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, "Not Found", ex.Message);
        }
        catch (BusinessException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, "Business Error", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "Internal Server Error", "Beklenmeyen bir hata oluştu.");
        }
    }

    private static Task WriteAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            status = statusCode,
            title,
            detail
        });

        return context.Response.WriteAsync(payload);
    }
}
