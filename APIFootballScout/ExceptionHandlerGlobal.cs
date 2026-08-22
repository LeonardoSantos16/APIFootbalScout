using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout
{
    public class ExceptionHandlerGlobal(
        ILogger<ExceptionHandlerGlobal> logger,
        IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                logger.LogInformation("Request cancelled by the client: {Path}", httpContext.Request.Path);
                return true;
            }

            if (httpContext.Response.HasStarted)
            {
                logger.LogError(exception, "Exception thrown after the response had started: {Path}", httpContext.Request.Path);
                return false;
            }

            var (status, titulo) = MapearStatus(exception);

            LogException(status, exception);

            httpContext.Response.StatusCode = status;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = titulo,
                    Detail = status == StatusCodes.Status500InternalServerError
                        ? "An unexpected error occurred while processing the request."
                        : exception.Message,
                    Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
                }
            });
        }

        private static (int status, string titulo) MapearStatus(Exception exception) => exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operation conflict"),
            ArgumentNullException => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        private void LogException(int status, Exception exception)
        {
            if (status >= StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unexpected error: {Message}", exception.Message);
            else
                logger.LogWarning("Handled exception [{Status}]: {Message}", status, exception.Message);
        }
    }
}
