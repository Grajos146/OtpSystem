namespace OtpSystem.Infrastructure.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(
        ILogger<ExceptionHandlingMiddleware> logger,
        RequestDelegate next
    )
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");

            await HandleExceptionAsync(context);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Detail = "An unexpected error occured, please try again later.",
            Instance = context.Request.Path,
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
