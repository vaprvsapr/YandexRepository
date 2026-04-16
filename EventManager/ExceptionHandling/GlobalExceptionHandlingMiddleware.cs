using System.ComponentModel.DataAnnotations;

namespace EventManager.ExceptionHandling;

/// <summary>
/// Глобальный middleware для обработки необработанных исключений в ASP.NET Core приложении. 
/// Этот middleware перехватывает все исключения, которые не были обработаны в других местах приложения, 
/// и возвращает стандартизированный JSON-ответ с информацией об ошибке. Он также логирует детали исключения, 
/// включая HTTP метод, путь запроса и идентификатор запроса, что помогает в диагностике и отладке проблем в приложении.
/// </summary>
/// <param name="next"></param>
/// <param name="logger"></param>
public class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger = logger;

    /// <summary>
    /// Метод, который вызывается для каждого HTTP запроса. 
    /// Он оборачивает выполнение следующего middleware в конвейере в блок try-catch,
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleException(httpContext, ex);
        }
    }

    private async Task HandleException(HttpContext httpContext, Exception ex)
    {
        _logger.LogError(
                ex,
                "Unhandled exception. Method={Method}, Path={Path}, RequestId={RequestId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Request.Headers["x-request-id"]);

        if (httpContext.Response.HasStarted)
            return;

        var statusCode = MapStatusCode(ex);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var error = new ProblemDetails
        {
            Status = statusCode,
            Detail = ex.Message
        };

        await httpContext.Response.WriteAsJsonAsync(error);
    }

    private static int MapStatusCode(Exception ex) => ex switch
    {
        ValidationException => StatusCodes.Status400BadRequest,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status500InternalServerError
    };
}
