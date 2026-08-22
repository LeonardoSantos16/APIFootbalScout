using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Infrastructure.External;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout
{
    public class ExceptionHandlerGlobal(
        ILogger<ExceptionHandlerGlobal> logger,
        IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        private const string DetalheGenerico = "An unexpected error occurred while processing the request.";

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

            var erroConhecido = exception as ICodigoDeErro;

            var problemDetails = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = erroConhecido is null ? DetalheGenerico : exception.Message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };

            if (erroConhecido is not null)
                problemDetails.Extensions["code"] = erroConhecido.Codigo;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
        }

        private static (int status, string titulo) MapearStatus(Exception exception) => exception switch
        {
            RecursoNaoEncontradoException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflitoDeDominioException => (StatusCodes.Status409Conflict, "Operation conflict"),
            RegraDeNegocioException => (StatusCodes.Status422UnprocessableEntity, "Business rule violation"),
            ValorInvalidoException => (StatusCodes.Status400BadRequest, "Invalid request"),
            FonteExternaIndisponivelException => (StatusCodes.Status502BadGateway, "External data source unavailable"),
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
